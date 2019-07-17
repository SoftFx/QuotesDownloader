# QuotesDownloader

### **Command Line Usage**<br />
#### **Command line arguments:**<br />
**a|address** –  address for connection establishment<br />
**u|username**  –  username for connection establishment<br />
**w|password** – password for connection establishment<br />
**p|port** –  port number for connection establishment<br />
**h|?|help** – help with command line arguments<br />
**t|dateto** – date to<br />
**f|datefrom** – date from<br />
**s|symbol** – symbol of quotes/bars<br />
**d|periodicity** – periodicity (H1, M15, etc.; for bars only)<br />
**o|output** – output type (csv, hdf5, csv_zip)<br />
**l|location** – folder for saving quotes<br />
**v|verbose** – display log information while running<br />
**r|request** – bids or asks for bars; ticks, level2, VWAP for quotes<br />
#### **Default values:**<br />
**dateto** – the current day – 7days<br />
**datefrom**  – the current day – 14days<br />
**symbol** – EURUSD<br />
**periodicity** – H1 <br />
**request** – ticks (bids for bars) <br />
**verbose** – false (silent mode)<br />
**outputType** – csv<br />
**location** – {MyDocuments_folder}\Quotes<br />
#### **Examples:**<br />
To download bids bars for EURAUD for every hour from June 4th to June 5th in text format & silent mode:<br />
```-a=ttlive.soft-fx.com -p=5050 -u=111 -w=123456 -s=EURAUD -r=Bids -d=H1 -f=2018-06-04 -t=2018-06-05```<br /><br />
To download quotes for EURUSD from June 7th to June 8th in hdf5 format & verbose mode:<br />
```-a=ttlive.soft-fx.com -p=5050 -u=111 -w=123456 -s=EURUSD -r=ticks -f=2018-06-07 -t=2018-06-08 -o=hdf5 -v=true```<br />

#### **Csv format description:**<br />
All dates and times are in format **"yyyy.MM.dd HH:dd:ss.fff[-ind if ind>0]"** example:<br /> 
"2019.02.08 10:06:37.902"<br />
"2019.02.08 10:06:53.889-2"<br />

**Csv row formmatting** <br />

ticks and vwap ticks: <br />
"date,bidPrice,bidVolume,askPrice,askVolume"<br />
if side is not presented corresponding values are not written.<br />

#### **Example:**<br /> 
"2019.02.22 10:34:15.567,0.92,1251000,1.02,1000000"<br />
"2019.02.22 10:34:20.176,,,1,100000"<br />
"2019.02.22 10:35:06.364,0.9,500000,,"<br />
"2019.02.22 10:35:20.649,,,,"<br />
