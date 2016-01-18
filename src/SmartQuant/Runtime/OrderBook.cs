﻿using System;
using System.Collections.Generic;
using System.Linq;

namespace SmartQuant
{
    public class OrderBook
    {
        public IList<Tick> Bids { get; } = new List<Tick>();

        public IList<Tick> Asks { get; } = new List<Tick>();

        public int GetAskVolume() => this.GetTotalSize(Asks);

        public double GetAvgAskPrice() => GetAvgPx(Asks);

        public double GetAvgBidPrice() => GetAvgPx(Bids);

        public int GetBidVolume() => GetTotalSize(Bids);

        public Quote GetQuote(int level)
        {
            var bid = new Bid();
            var ask = new Ask();
            Tick tick;
            if (Bids.Count < level)
            {
                tick = Bids[level];
                bid = new Bid(tick.DateTime, tick.ProviderId, tick.InstrumentId, tick.Price, tick.Size);
            }
            if (Asks.Count < level)
            {
                tick = Asks[level];
                ask = new Ask(tick.DateTime, tick.ProviderId, tick.InstrumentId, tick.Price, tick.Size);
            }
            return new Quote(bid, ask);
        }

        internal void method_0(Level2Snapshot l2s)
        {
            Bids.Clear();
            Asks.Clear();
            l2s.Bids.ToList().ForEach(b => Bids.Add(new Tick(b)));
            l2s.Asks.ToList().ForEach(a => Asks.Add(new Tick(a)));
        }

        internal void method_1(Level2Update l2u)
        {
            for (int i = 0; i < l2u.Entries.Length; i++)
            {
                var level = l2u.Entries[i];
                IList<Tick> list = null;
                switch (level.Side)
                {
                    case Level2Side.Bid:
                        list = Bids;
                        break;
                    case Level2Side.Ask:
                        list = Asks;
                        break;
                }
                switch (level.Action)
                {
                    case Level2UpdateAction.New:
                        list.Insert(level.InstrumentId, new Tick(level));
                        break;
                    case Level2UpdateAction.Change:
                        list[level.InstrumentId].Size = level.Size;
                        break;
                    case Level2UpdateAction.Delete:
                        if (level.InstrumentId >= list.Count)
                        {
                            Console.WriteLine($"OrderBook:: {level.Side}  Delete warning at index: {level.InstrumentId}, max index: {list.Count - 1}, InstrumentId: {level.InstrumentId}");
                            list.RemoveAt(list.Count - 1);
                        }
                        else
                        {
                            list.RemoveAt(level.InstrumentId);
                        }
                        break;
                    case Level2UpdateAction.Reset:
                        list.Clear();
                        break;
                }
            }
        }

        private int GetTotalSize(IList<Tick> ticks) => ticks.Sum(t => t.Size);

        private double GetAvgPx(IList<Tick> ticks)
        {
            double sum = 0;
            double size = 0;
            foreach (var t in ticks)
            {
                sum += t.Price * t.Size;
                size += t.Size;
            }
            return sum / size;
        }
    }
}