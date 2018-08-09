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


def read_bars(filename, index_from=0, index_to=-1):
    """ output: 2 arrays
    bars - 2-dimensional array, where the first dimension is index number, the second contains the bars info:
    0 - volume, 1 - open, 2 - close, 3- high, 4 - low;
    bars data - 2-dimensional array, where the first dimension is index number, the second contains the bars data info:
    0 - date (timestamp from Jan 1, 1970 in milliseconds), 1 - type (bid - 0 / ask - 1)
    """

    f = h5py.File(filename, 'r')
    bars_dataset = f["Bars"]
    data_bars_dataset = f["DataBars"]

    if index_to == -1:
        index_to = bars_dataset.shape[0]

    bars = bars_dataset[index_from:index_to]
    bars_data = data_bars_dataset[index_from:index_to]

    return bars, bars_data


def read_quotes(filename, index_from=0, index_to=-1):
    """ output: 2 arrays
    quotes - 3-dimensional array, where the first dimension is index number, the second one shows asks(0) & bids(1) quotes,
    the third one contains the quotes info: 0 - volume, 1 - price (2 - volume, 3 - price for next quote, etc. for level 2)
    quotes date - 1-dimensional array, which contains the quotes date info (timestamp from Jan 1, 1970 in milliseconds):
    """

    f = h5py.File(filename, 'r')
    quotes_dataset = f["Quotes"]
    date_quotes_dataset = f["DateQuotes"]

    if index_to == -1:
        index_to = quotes_dataset.shape[0]

    quotes = quotes_dataset[index_from:index_to]
    quotes_date = date_quotes_dataset[index_from:index_to]

    return quotes, quotes_date


def read_bars_classes(filename, index_from=0, index_to=-1):
    f = h5py.File(filename, 'r')
    bars_dataset = f["Bars"]
    data_bars_dataset = f["DataBars"]

    if index_to == -1:
        index_to = bars_dataset.shape[0]

    bars = list(bars_dataset[index_from:index_to])
    bars_data = list(data_bars_dataset[index_from:index_to])

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


def read_quotes_classes(filename, index_from=0, index_to=-1):
    f = h5py.File(filename, 'r')
    quotes_dataset = f["Quotes"]
    date_quotes_dataset = f["DateQuotes"]

    if index_to == -1:
        index_to = quotes_dataset.shape[0]

    quotes = list(quotes_dataset[index_from:index_to])
    quotes_date = list(date_quotes_dataset[index_from:index_to])

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


def get_datetime_indices(filename, dataset_name, datetime_from, datetime_to):
    f = h5py.File(filename, 'r')
    dataset = f[dataset_name]
    deep = True if len(dataset.shape) > 1 else False
    index_from = bin_search(dataset, datetime_from.timestamp() * 1000, True, deep)
    index_to = bin_search(dataset, datetime_to.timestamp() * 1000, False, deep)
    f.close()
    return index_from, index_to


def bin_search(dataset, value, lte, deep):
    lower_bound = 0
    upper_bound = dataset.shape[0]
    while lower_bound != upper_bound:
        compared_value = (lower_bound + upper_bound) // 2
        pivot = dataset[compared_value, 1] if deep else dataset[compared_value]
        cmp = value <= pivot if lte else value < pivot
        if cmp:
            upper_bound = compared_value
        else:
            lower_bound = compared_value + 1
    return lower_bound


if __name__ == "__main__":
    # Quotes selective reading (by datetime)
    filename = 'AUDJPY 20180608 20180609.h5'
    datetime_from = datetime.datetime(2018, 6, 8, 5, 0, 0, tzinfo=pytz.UTC)
    datetime_to = datetime.datetime(2018, 6, 8, 9, 0, 0, tzinfo=pytz.UTC)
    index_from, index_to = get_datetime_indices(filename, "DateQuotes", datetime_from, datetime_to)
    quotes, quotes_date = read_quotes(filename, index_from, index_to)

    # Bars selective reading (by datetime)
    filename = 'EURUSD Bid S10  20180615  20180616.h5'
    datetime_from = datetime.datetime(2018, 6, 15, 5, 0, 0, tzinfo=pytz.UTC)
    datetime_to = datetime.datetime(2018, 6, 15, 9, 0, 0, tzinfo=pytz.UTC)
    index_from, index_to = get_datetime_indices(filename, "DataBars", datetime_from, datetime_to)
    bars, bars_data = read_bars(filename, index_from, index_to)


