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
**o|output** – output type (cvs or hdf5)<br />
**l|location** – folder for saving quotes<br />
**v|verbose** – display log information while running<br />
**r|request** – bids or asks for bars; ticks or level2 for quotes<br />
#### **Default values:**<br />
**dateto** – the current day – 7days<br />
**datefrom**  – the current day – 14days<br />
**symbol** – EURUSD<br />
**periodicity** – H1 <br />
**request** – ticks (bids for bars) <br />
**verbose** – false (silent mode)<br />
**outputType** – text<br />
**location** – {MyDocuments_folder}\Quotes<br />
#### **Examples:**<br />
To download bids bars for EURAUD for every hour from June 4th to June 5th in text format & silent mode:<br />
```-a=ttlive.soft-fx.com -p=5050 -u=111 -w=123456 -s=EURAUD -r=Bids -d=H1 -f=2018-06-04 -t=2018-06-05```<br /><br />
To download quotes for EURUSD from June 7th to June 8th in hdf5 format & verbose mode:<br />
```-a=ttlive.soft-fx.com -p=5050 -u=111 -w=123456 -s=EURUSD -r=ticks -f=2018-06-07 -t=2018-06-08 -o=hdf5 -v=true```<br />
