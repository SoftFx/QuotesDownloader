#' R6 Class Representing a TickTrader QuotesDownloader Tool
#'@importFrom R6 R6Class
#'@import data.table
#'@importFrom lubridate days ymd_hms
#'@description
#'A R Client for TickTrader Quote Downlodaer
#'
#'@details
#'A R Client to Download Quote History from TickTrader
#'@export
RTTQDClient <- R6Class("RTTQDClient",
private = list(
  # #' @field Server. Address to Download QuoteHistory from.
  .server = NULL,
  # #' @field Client Login
  .login = NULL,
  # #' @field TT Client Password.
  .password = NULL,
  # #' @field Folder Path to save downloaded quoteHistory Files.
  .quoteStorage =  NULL,
  DownloadQuotes = function(symbol, startDate, endDate, periodicity, type){
    parameters <- sprintf("-a=%s -p=5042 -u=%s -w=%s -s=%s -f=%s -t=%s -o=csv -v=false -d=%s -l=\"%s\" -r=%s",
                          private$.server, private$.login, private$.password, symbol, startDate, endDate, periodicity, private$.quoteStorage, type)
    # fileName <- file.path("C:\\Users\\yury.maisiayonak\\Desktop\\RTTQDClient\\data\\QuotesDownloader", "QuotesDownloader.exe")
    fileName <- system.file("QuotesDownloader", "QuotesDownloader.exe", package = "RTTQDClient")
    tryCatch({
      system2(fileName, parameters, stdout = TRUE, timeout = 200)
    }, warning = function(w){
      if(grepl("timed out", w$message)){
        stop(paste("Time out. Please check your TTS client credentials"))
      }else{
        message("Warning", w$message)
      }
    },
    error = function(e){
      stop(paste("Error", e$message))
    })
  },
  GetBars = function(symbol, startDate = as.Date("2020-01-01"), endDate = as.Date("2020-01-02"), periodicity="M1", type="Bids"){
    qdFileName <- file.path(private$.quoteStorage, paste0(symbol, " ", substr(type, 0, 3), " ", periodicity, "  ", format(startDate, "%Y%m%d"), "  ", format(endDate, "%Y%m%d"), ".csv"))
    if( !file.exists(qdFileName) ){
      private$DownloadQuotes(symbol, startDate, endDate, periodicity, type)
    }
    d <- fread(qdFileName, col.names = c("date_time", "open", "close", "low", "high", "volume"))
    d[,date_time:=ymd_hms(date_time)]
    return(d)
  },
  periodicities = c("S1", "S10", "M1", "M5", "M15", "M30", "H1", "H4", "D1", "W1", "MN1"),
  types = c("Bids", "Asks", "BidAsk"),
  finalize = function() {
    print("remove RQD object")
  }
),
public = list(
  #' @description
  #' Create RTTQDClient object.
  #' @param server Server
  #' @param login Login
  #' @param password Password
  #' @param quoteStorage QuoteStorage Path
  #' @return A new RTTQDClient object.
  initialize = function(server = "ttlive.fxopen.com", login = "100", password = "TTqfdeppmhDR", quoteStorage =  normalizePath(".//Quotes")){
    private$.server <- server
    private$.login <- login
    private$.password <- password
    private$.quoteStorage <- quoteStorage
  },
  #' @description
  #' Get Bar History
  #' @param symbol SymbolName.
  #' @param startDate StartDate. Date object
  #' @param endDate EndDate. Date object
  #' @param periodicity Periodicity. One from c("S1", "S10", "M1", "M5", "M15", "M30", "H1", "H4", "D1", "W1", "MN1")
  #' @param type Type. One from c("Bids", "Asks", "BidAsk")
  #' @return data.table with BarsHistory
  #' @examples
  #' qdClient <- RTTQDClient$new()
  #' bars <- qdClient$GetBarsHistory("EURUSD", as.Date("2020-01-01"), as.Date("2020-01-02"), "M1", "Bids")
  GetBarsHistory = function(symbol, startDate = as.Date("2020-01-01"), endDate = as.Date("2020-01-02"), periodicity="M1", type="Bids") {
    if(!is.element(periodicity, private$periodicities))
      stop(paste("Wrong periodicity", periodicity, "Use one from", paste(private$periodicities)))
    if(!is.element(type, c("Bids", "Asks", "BidAsk")))
      stop(paste("Wrong Bar Type", private$types, "Use one from", paste(private$types)))
    if(type == "BidAsk"){
      asks <- private$GetBars(symbol, startDate, endDate, periodicity, "Asks")
      bids <- private$GetBars(symbol, startDate, endDate, periodicity, "Bids")
      res <- merge(asks, bids, by="date_time", suffixes = c(".ask", ".bid"), all = TRUE)
      setnafill(res, type=c("locf"), cols=seq_along(res))
      setnafill(res, type=c("nocb"), cols=seq_along(res))
      return(res)
    }
    return(private$GetBars(symbol, startDate, endDate, periodicity, type))
  },
  #' @description
  #' Get Ticks History
  #' @param symbol SymbolName.
  #' @param startDate StartDate. Date object
  #' @param endDate EndDate. Date object.
  #' @return data.table with TicksHistory
  #' @examples
  #' qdClient <- RTTQDClient$new()
  #' ticks <- qdClient$GetTickHistory("EURUSD", as.Date("2020-01-01"), as.Date("2020-01-02"))
  GetTickHistory = function(symbol, startDate = Sys.Date() - days(1), endDate = Sys.Date()) {
    qdFileName <- file.path(private$.quoteStorage, paste0(symbol, " ", format(startDate, "%Y%m%d"), " ", format(endDate, "%Y%m%d"), ".csv"))
    if( !file.exists(qdFileName) ){
      private$DownloadQuotes(symbol, startDate, endDate, "", type = "ticks")
    }
    d <- fread(qdFileName, sep = ";", dec = ",", skip = 1, header = FALSE, col.names = c("date_time", "bid_price", "bid_volume", "ask_price", "ask_volume"))
    d[, date_time := ymd_hms(date_time)]
    return(d)
  },

  #' @description
  #'Clean local Storage
  #' @param path. Set if your local
  CleanStorage = function(path = ""){
    "Clean Storage"
    unlink(private$.quoteStorage, recursive = TRUE, force = TRUE)
  }
),
active = list(

  #' @description
  #' QuoteStorage Property. Set value (if exist) or return a current value.
  #'
  quoteStorage = function(value) {
    if (missing(value)) {
      private$.quoteStorage
    } else {
      stopifnot(is.character(value), length(value) == 1)
      private$.quoteStorage <- value
      self
    }
  }
)
)
#
# server = "ttlive.fxopen.com"
# login = "100"
# password = "TTqfdeppmhDR"
# quoteStorage =  normalizePath(".//Quotes")
# symbol <- "EURUSD"
# startDate <- as.Date("2021-06-01")
# endDate <- as.Date("2021-06-02")
# periodicity <- "M1"
# type <- "Asks"

# rQDClient <- function(server = "ttlive.fxopen.com", login = "100", password = "TTqfdeppmhDR", quoteStorage =  normalizePath(".//Quotes")){
#   DownloadQuotes = function(symbol, startDate, endDate, periodicity, type){
#     parameters <- sprintf("-a=%s -p=5042 -u=%s -w=%s -s=%s -f=%s -t=%s -o=csv -v=false -d=%s -l=\"%s\" -r=%s",
#                           server, login, password, symbol, startDate, endDate, periodicity, quoteStorage, type)
#     fileName <- file.path("C:\\Users\\yury.maisiayonak\\Desktop\\RTTQDClient\\data\\QuotesDownloader", "QuotesDownloader.exe")
#     tryCatch({
#       system2(fileName, parameters, stdout = TRUE, timeout = 200)
#     }, warning = function(w){
#       if(grepl("timed out", w$message)){
#         stop(paste("Time out. Please check your TTS client credentials"))
#       }else{
#         message("Warning", w$message)
#       }
#     },
#     error = function(e){
#       stop(paste("Error", e$message))
#     })
#   }
#   GetBarHistory = function(symbol, startDate=ISOdate(2019, 1, 1, tz = "UTC"), endDate=ISOdate(2020, 1, 1, tz = "UTC"), periodicity="M1", type="Bids"){
#     if( type == "BidAsk"){
#       asks <- GetBarHistory(symbol, startDate, endDate, periodicity, "Asks")
#       bids <- GetBarHistory(symbol, startDate, endDate, periodicity, "Bids")
#       res <- merge(asks, bids, by="date_time", suffixes = c(".ask", ".bid"))
#       setnafill(res, type=c("locf"), cols=seq_along(res))
#       setnafill(res, type=c("nocb"), cols=seq_along(res))
#       return(res)
#     }
#     qdFileName <- file.path(quoteStorage, paste0(symbol, " ", substr(type, 0, 3), " ", periodicity, "  ", format(startDate, "%Y%m%d"), "  ", format(endDate, "%Y%m%d"), ".csv"))
#     if( !file.exists(qdFileName) ){
#       DownloadQuotes(symbol, startDate, endDate, periodicity, type)
#     }
#     d <- fread(qdFileName, col.names = c("date_time", "open", "close", "low", "high", "volume"))
#     d[,date_time:=ymd_hms(date_time)]
#     return(d)
#   }
#   GetTickHistory = function(symbol, startDate, endDate) {
#     qdFileName <- file.path(quoteStorage, paste0(symbol, " ", format(startDate, "%Y%m%d"), " ", format(endDate, "%Y%m%d"), ".csv"))
#     if( !file.exists(qdFileName) ){
#       DownloadQuotes(symbol, startDate, endDate, "", type = "ticks")
#     }
#     d <- fread(qdFileName, sep = ";", dec = ",", skip = 1, header = FALSE, col.names = c("date_time", "bid_price", "bid_volume", "ask_price", "ask_volume"))
#     d[, date_time := ymd_hms(date_time)]
#     return(d)
#   }
#   return(list(GetTickHistory = GetTickHistory, GetBarHistory = GetBarHistory))
# }
