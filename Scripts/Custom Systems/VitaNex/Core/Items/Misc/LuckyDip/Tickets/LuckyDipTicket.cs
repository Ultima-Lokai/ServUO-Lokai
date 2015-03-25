﻿#region Header
//   Vorspire    _,-'/-'/  LuckyDipTicket.cs
//   .      __,-; ,'( '/
//    \.    `-.__`-._`:_,-._       _ , . ``
//     `:-._,------' ` _,`--` -: `_ , ` ,' :
//        `---..__,,--'  (C) 2014  ` -'. -'
//        #  Vita-Nex [http://core.vita-nex.com]  #
//  {o)xxx|===============-   #   -===============|xxx(o}
//        #        The MIT License (MIT)          #
#endregion

#region References
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

using Server;
#endregion

namespace VitaNex.Items
{
	public class LuckyDipTicket : Item
	{
		private static void Normalize(ref double chance)
		{
			chance = Math.Max(0.0, Math.Min(1.0, chance));
		}

		private int _LuckCap = 3000;
		private int _PrizeTier;

		public List<LuckyDipPrize> Prizes { get; set; }

		[CommandProperty(AccessLevel.Counselor, AccessLevel.GameMaster)]
		public int PrizeTier
		{
			get { return _PrizeTier; }
			set
			{
				value = Math.Max(1, value);

				if (_PrizeTier == value)
				{
					return;
				}

				_PrizeTier = value;
				InitPrizes();
				InvalidateProperties();
			}
		}

		[CommandProperty(AccessLevel.Counselor, AccessLevel.GameMaster)]
		public int LuckCap
		{
			get { return _LuckCap; }
			set
			{
				value = Math.Max(0, value);

				if (_LuckCap == value)
				{
					return;
				}

				_LuckCap = value;
				InvalidateProperties();
			}
		}

		[CommandProperty(AccessLevel.Counselor, AccessLevel.GameMaster)]
		public virtual int GoldPrice { get { return (int)Math.Ceiling(GetAveragePrizeWorth()); } }

		[Constructable]
		public LuckyDipTicket()
			: this(1)
		{ }

		[Constructable]
		public LuckyDipTicket(int tierMin, int tierMax)
			: this(Utility.RandomMinMax(tierMin, tierMax))
		{ }

		[Constructable]
		public LuckyDipTicket(int tier)
			: base(0x14F0)
		{
			PrizeTier = tier;

			Name = "Lucky Dip Ticket";
			Weight = 1.0;
			LootType = LootType.Blessed;
		}

		public LuckyDipTicket(Serial serial)
			: base(serial)
		{ }

		public void InitPrizes()
		{
			if (Prizes == null)
			{
				Prizes = new List<LuckyDipPrize>();
			}

			Prizes.Clear();
			InitBankChecks();
			InitItems();
		}

		protected virtual void InitItems()
		{ }

		protected virtual void InitBankChecks()
		{
			int f = (int)Math.Max(0, Math.Pow(10, PrizeTier));

			AddPrize(new LuckyDipBankCheckPrize(0.80, 1 * f));
			AddPrize(new LuckyDipBankCheckPrize(0.40, 10 * f));
			AddPrize(new LuckyDipBankCheckPrize(0.20, 25 * f));
			AddPrize(new LuckyDipBankCheckPrize(0.10, 50 * f));
			AddPrize(new LuckyDipBankCheckPrize(0.05, 75 * f));
			AddPrize(new LuckyDipBankCheckPrize(0.02, 100 * f));
		}

		public virtual int GetMinPrizeWorth()
		{
			return Prizes.OfType<LuckyDipBankCheckPrize>().Min(e => e.Worth);
		}

		public virtual int GetMaxPrizeWorth()
		{
			return Prizes.OfType<LuckyDipBankCheckPrize>().Max(e => e.Worth);
		}

		public virtual double GetAveragePrizeWorth()
		{
			return Prizes.OfType<LuckyDipBankCheckPrize>().Average(e => e.Worth);
		}

		public double GetMinChance()
		{
			return Prizes.Min(e => e.Chance);
		}

		public double GetMaxChance()
		{
			return Prizes.Max(e => e.Chance);
		}

		public double GetAverageChance()
		{
			return Prizes.Average(e => e.Chance);
		}

		public void AddPrize(LuckyDipPrize prize)
		{
			if (!Prizes.Contains(prize) && OnAddPrize(prize))
			{
				Prizes.Add(prize);
			}
		}

		protected virtual bool OnAddPrize(LuckyDipPrize prize)
		{
			return true;
		}

		public virtual LuckyDipPrize[] GetPrizes(double chance)
		{
			if (Prizes == null || Prizes.Count == 0)
			{
				return new LuckyDipPrize[0];
			}

			Normalize(ref chance);

			var p = Prizes.Where(e => !e.Disabled && e.Chance <= chance).OrderByDescending(e => e.Chance).ToArray();

			//p.ForEach(o => Console.WriteLine("Prize: T: {0} C: {1}", o.Type, o.Chance));

			return p;
		}

		public virtual LuckyDipPrize GetPrize(double chance)
		{
			if (Prizes == null || Prizes.Count == 0)
			{
				return LuckyDipPrize.Empty;
			}

			Normalize(ref chance);

			return GetPrizes(chance).Where(p => Utility.RandomDouble() * chance <= p.Chance).GetRandom();
		}

		protected void BeginGamble(Mobile from)
		{
			if (from == null || from.Deleted)
			{
				return;
			}

			if (!from.BeginAction(GetType()))
			{
				from.SendMessage(34, "You must wait a moment before using another ticket.");
				return;
			}

			from.SendMessage(85, "Please wait a moment while we process your ticket...");
			Timer.DelayCall(TimeSpan.FromSeconds(2.0), EndGamble, from);
		}

		protected void EndGamble(Mobile from)
		{
			if (from == null || from.Deleted)
			{
				return;
			}

			from.EndAction(GetType());

			double a = Utility.RandomDouble();
			double b = Math.Min(LuckCap, from.Luck) / (double)LuckCap;
			double c = a + b;

			Normalize(ref c);

			//Console.WriteLine("LDT: A = {0} B = {1} C = {2}", a, b, c);

			LuckyDipPrize prizeEntry = GetPrize(c);

			if (prizeEntry == null || prizeEntry.Disabled || prizeEntry.Equals(LuckyDipPrize.Empty))
			{
				from.SendMessage(34, "Sorry {0}, you didn't win anything, better luck next time!", from.RawName);
				Delete();
				return;
			}

			Item prize = prizeEntry.CreateInstance<Item>();

			if (prize == null)
			{
				Prizes.Remove(prizeEntry);

				VitaNexCore.ToConsole(
					"WARNING: An instance of {0} could not be constructed in {1} at {2}",
					prizeEntry.Type.FullName,
					GetType().FullName,
					prizeEntry.GetType().FullName);

				from.SendMessage(34, "We couldn't process your ticket, please try again.");
				return;
			}

			from.SendMessage(85, "Congratulations, you won a prize! ({0})", prize.ResolveName(from));
			ReplaceWith(prize);
		}

		public override void OnDoubleClick(Mobile from)
		{
			if (this.CheckDoubleClick(from, true, false, 2, true))
			{
				BeginGamble(from);
			}
		}

		public override void GetProperties(ObjectPropertyList list)
		{
			base.GetProperties(list);

			string props = String.Format("Use: A chance to win a Tier {0} prize!".WrapUOHtmlColor(Color.SkyBlue), PrizeTier);

			if (LuckCap > 0)
			{
				props += "\n" + "Tip: Increase your luck to increase your odds!".WrapUOHtmlColor(Color.Gold);
			}

			list.Add(props);
		}

		public override void Serialize(GenericWriter writer)
		{
			base.Serialize(writer);

			int version = writer.SetVersion(0);

			switch (version)
			{
				case 0:
					{
						writer.Write(LuckCap);
						writer.Write(PrizeTier);
					}
					break;
			}
		}

		public override void Deserialize(GenericReader reader)
		{
			base.Deserialize(reader);

			int version = reader.GetVersion();

			switch (version)
			{
				case 0:
					{
						LuckCap = reader.ReadInt();
						PrizeTier = reader.ReadInt();
					}
					break;
			}
		}
	}
}