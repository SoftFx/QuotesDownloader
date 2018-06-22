import numpy as np
import h5py
import datetime
import pytz


class Bar:
    def __init__(self, date_from, price_type, volume, open, close, high, low):
        self.date_from = date_from
        self.price_type = price_type
        self.volume = volume
        self.open = open
        self.close = close
        self.high = high
        self.low = low


class Quote:
    def __init__(self, date, asks, bids):
        self.date = date
        self.asks = asks
        self.bids = bids


class QuoteEntry:
    def __init__(self, price, volume):
        self.price = price
        self.volume = volume


def read_bars(filename):
    f = h5py.File(filename, 'r')
    bars = list(f["Bars"])
    bars_data = list(f["DataBars"])
    bars_list = []
    for i in range(len(bars)):
        bars_list.append(Bar(date_from=datetime.datetime.fromtimestamp(bars_data[i][1] / 1e3, tz=pytz.UTC),
                             price_type=bars_data[i][0],
                             volume=bars[i][0],
                             open=bars[i][1],
                             close=bars[i][2],
                             high=bars[i][3],
                             low=bars[i][4]
                             ))
    return bars_list


def read_quotes(filename):
    f = h5py.File(filename, 'r')
    quotes = list(f["Quotes"])
    quotes_date = list(f["DateQuotes"])
    quotes_list = []
    for i in range(len(quotes)):
        asks = []
        bids = []
        index = 0
        for j in range(quotes[0][0].size // 2):
            asks.append(QuoteEntry(volume=quotes[i][0][index], price=quotes[i][0][index + 1]))
            index += 2
        index = 0
        for j in range(quotes[0][1].size // 2):
            bids.append(QuoteEntry(volume=quotes[i][1][index], price=quotes[i][1][index + 1]))
            index += 2
        quotes_list.append(Quote(date=datetime.datetime.fromtimestamp(quotes_date[i] / 1e3, tz=pytz.UTC), bids=bids, asks=asks))
    return quotes_list


if __name__ == "__main__":
    filename = 'AUDJPY 20180608 20180609.h5'
    f = h5py.File(filename, 'r')
    quotes_list = read_quotes(filename)


