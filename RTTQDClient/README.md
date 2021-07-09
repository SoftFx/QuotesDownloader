# `RTTQDClient`

R6 Class Representing a TickTrader QuotesDownloader Tool


## Description

A R Client for TickTrader Quote Downlodaer


## Details

A R Client to Download Quote History from TickTrader


## Examples

```r
## ------------------------------------------------
## Method `RTTQDClient$GetBarsHistory`
## ------------------------------------------------

qdClient <- RTTQDClient$new()
bars <- qdClient$GetBarsHistory("EURUSD", as.Date("2020-01-01"), as.Date("2020-01-02"), "M1", "Bids")

## ------------------------------------------------
## Method `RTTQDClient$GetTickHistory`
## ------------------------------------------------

qdClient <- RTTQDClient$new()
ticks <- qdClient$GetTickHistory("EURUSD", as.Date("2020-01-01"), as.Date("2020-01-02"))
```


