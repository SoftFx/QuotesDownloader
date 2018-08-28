library(data.table)
library(lubridate)
library(rhdf5)

options(digits.secs = 6)

readBars <- function(filePath = getwd(), fileName) {
  bars = h5read(file.path(filePath, fileName), "Bars")
  barsData = h5read(file.path(filePath, fileName), "DataBars", bit64conversion = "double")
  
  bars = transpose(data.table(bars))
  barsData = transpose(data.table(barsData))
  
  colnames(bars) <- c("Volume", "Open", "Close", "High", "Low")
  colnames(barsData) <- c("barType", "Date")
  barsData$Date = barsData$Date/1000
  barsData$Date = as.POSIXct(barsData$Date, origin = "1970-01-01", tz = "UTC")
  bars = cbind(barsData, bars)
  bars <- bars[,barType:=ifelse(barType==0, "Bid", "Ask")]
  setkey(bars, "Date")
}

readQuotes <- function(filePath = getwd(), fileName) {
  quotes = h5read(file.path(filePath, fileName), "Quotes")
  bid = quotes[,1,]
  bid = transpose(as.data.table(bid))
  colnames(bid) <- c("BidPrice", "BidVolume")
  
  ask = quotes[,2,]
  ask = transpose(as.data.table(ask))
  colnames(ask) <- c("AskPrice", "AskVolume")
  
  quotesDate = h5read(file.path(filePath, fileName), "DateQuotes", bit64conversion = "double")
  quotesDate = data.table(quotesDate)
  colnames(quotesDate) <- c("Timestamp")
  
  quotesDate$Timestamp = quotesDate$Timestamp/1000
  quotesDate$Timestamp <- as.POSIXct(quotesDate$Timestamp, origin = "1970-01-01", tz = "UTC")
  quotes = cbind(quotesDate, ask,bid)
  setkey(quotes, "Timestamp")
}

###Reading Ticks
ticks <- readQuotes(fileName = "AUDUSD 20180701 20180702.h5")
View(ticks)

###Reading Bars
bars <- readBars(fileName = "AUDUSD Bid H1  20180701  20180802.h5")
View(bars)
