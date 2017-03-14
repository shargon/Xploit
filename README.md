# Xploit
Open source exploit framework made in C#

# Youtube Sample Channel
https://www.youtube.com/playlist?list=PLJWNXwuU6PvyAcNk8AtF8abbrz5-Sd2pR

# Donations accepted

Paypal link:

<a href='https://www.paypal.me/Shargon/1'><img width='250' height='57' alt='Click here to lend your support to: XPloit Framework and make a donation at paypal !' src='http://www.theimho.org/sites/default/files/paypal-donate-button.gif' border='0' ></a>

Pledgie link:

<a href='https://pledgie.com/campaigns/31014'><img alt='Click here to lend your support to: XPloit Framework and make a donation at pledgie.com !' src='https://pledgie.com/campaigns/31014.png?skin_name=chrome' border='0' ></a>

Xploit Wiki
===================

Xploit opensource framework wiki written by <a href="https://www.twitter.com/alvarodh5">**@alvarodh5**</a> 

----------
<i class="icon-cog"></i>Commands
-------------

Commands available in **v4.0.30319 version**

<i class="icon-terminal"></i>**Help**: Displays a help text for the specified command, or displays a list of all available commands.
> **Use:**

>- Help command


<i class="icon-terminal"></i>**Back**: Un-use the current module
> **Use:**

>- cd / 
>- cd.. / 
>- back

<i class="icon-terminal"></i>**Banner**:  Show a beautiful xploit banner
> **Use:**

>- banner
>- ban (alias)

<i class="icon-terminal"></i>**Beep**:  Make a beep
> **Use:**

>- beep
>-  be (alias)


<i class="icon-terminal"></i>**Check**:  Check the current module
> **Use:**

>- check
>-  ch (alias)

>**Example:**
>> use Auxiliary/Multi/SSH/PortForwarding
>>>Module(PortForwarding)> check
>>>>[*] Its required to set the property Password (use set Password <value>)


<i class="icon-terminal"></i>**Clear**:  Clear console
> **Use:**

>- clear
>-  cls (alias)
>-  cle (alias)

<i class="icon-terminal"></i>**Echo**:  Print the input
> **Use:**

>- echo
>- ec (alias)

> **Example:**
>> echo This is Xploit!
>>>[*] This is Xploit!


<i class="icon-terminal"></i>**Exit**:  Exit xploit framework.
> **Use:**

>- exit
>- quit (alias)


<i class="icon-terminal"></i>**Exploit**:  Run the current module
> **Use:**

>- exploit
>- exp (alias)
>- run
>- rexploit
>- rex (alias)
>- rerun
>- rer (alias)


<i class="icon-terminal"></i>**Gset**:  Set a global variable for the current module and the next call to this module
> **Use:**

>- gset [variable] [value]
>- g [variable] [value]




<i class="icon-terminal"></i>**Help**:  Displays a help text for the specified command, or displays a list of all available commands.
> **Use:**

>- help
>- man
>- h (alias)
>- m (alias)
>- help [command]
>- man [command]

> **Example:**
>> help echo
>>>echo [input]
>>>Print the input


<i class="icon-terminal"></i>**Ifcheck**:  Check the module, and if works, then run the command

> **Use:**

>- ifcheck [command]
>- ifc (alias)




<i class="icon-terminal"></i>**Ifnocheck**:  Check the module, and if not works, then run the command

> **Use:**

>- ifnocheck [command]
>- ifnoc (alias)


<i class="icon-terminal"></i>**Ifrun**:  Run the module, and if works, then run the command

> **Use:**

>- ifrun [command]
>- ifr (alias)

<i class="icon-terminal"></i>**Ifnorun**:  Run the module, and if not works, then run the command

> **Use:**

>- ifnorun [command]
>- ifnor (alias)

<i class="icon-terminal"></i>**Info**:  Show info of the current module

> **Use:**

>- info
>- in (alias)

> **Example:**
>> info
>>>Path         Auxiliary/Multi/SSH
>>>Name         PortForwarding
>>>Author       Fernando Díaz Toledano
>>>Description  Port Forwarding from SSH machine


<i class="icon-terminal"></i>**Jobs**:  List all current jobs

> **Use:**

>- jobs
>- j (alias)



<i class="icon-terminal"></i>**Kill**:  Kill the selected job

> **Use:**

>- kill [job]
>- k (alias)


<i class="icon-terminal"></i>**Load**:  Load all modules from selected file

> **Use:**

>- load [file]
>- l (alias)


<i class="icon-terminal"></i>**Play**:  Run the commands stored in a file

> **Use:**

>- play [file]
>- p (alias)


<i class="icon-terminal"></i>**Rcheck**:  Reload the current module and check them

> **Use:**

>- rcheck
>- rc (alias)

<i class="icon-terminal"></i>**Record**:  Start/Stop recording the input to a file

> **Use:**

>- record [options]
>- rec (alias)

> **Options:**
>>- stop      Stop the current record
>>- [path]    Start a record in this path


> **Example:**
>>- record C:\myrecords\xploit.txt
>>>- record stop



<i class="icon-terminal"></i>**Reload**:  Reload the current module with the global variables

> **Use:**

>- reload
>- rel (alias)



<i class="icon-terminal"></i>**Search**:  Search a module in the loaded modules

> **Use:**

>- search
>- sea (alias)

> **Example:**
>>- search port
>>>Auxiliary/Multi/SSH/PortForwarding 
>>>Auxiliary/Local/Server/SocksPortForwarding 


<i class="icon-terminal"></i>**Set**: Set a variable for the current module
>- set

 > **Example:**
>>set [variable] [value]

<i class="icon-terminal"></i>**Show**: Show available information for the current module.
>- config/options --> Displays the config for current module
>- info -->  Display info of the current module
>- payloads -->  Display available payloads for the current module
>- targets --> Display available targets for the current module

<i class="icon-terminal"></i>**Use**: Use a XPloit module
>- use [module]
>- u (alias)

<i class="icon-terminal"></i>**Version**: Displays the current version of Xploit framework
>- version
>- v (alias)

----------


<i class="icon-bug"></i> Xploits
-------------------

Xploits available in **v4.0.30319 version**

#### <i class="icon-shield"></i> Auxiliary
Complete list of all Auxiliaries modules:


>- Auxiliary/Local/DatabaseQuery
>>Execute a query in a Database

>- Auxiliary/Local/DetectTorExitNode
>>Check if a IP its a Tor exit node

>- Auxiliary/Local/Exfiltration/DnsExfiltrate
>>DNS-Exfiltration send

>- Auxiliary/Local/Exfiltration/DnsExfiltrateParser
>>DNS-Exfiltration file parser

>- Auxiliary/Local/FileToHex
>>Create a Hex string from file

>- Auxiliary/Local/Fuzzing/PatternCreate
>>Generate pattern string for exploit development

>- Auxiliary/Local/Fuzzing/PatternSearch
>>Search pattern string for exploit development

>- Auxiliary/Local/Fuzzing/StreamFuzzer
>>Generic Fuzzer

>- Auxiliary/Local/NFC/MifareRestoreClone
>>Mifare Restore clone (dont touch Trailing blocks)

>- Auxiliary/Local/NFC/MifareSetId
>>Mifare Id Setter. Require a valid card

>- Auxiliary/Local/ProcessKill
>>Kill a process in local machine

>- Auxiliary/Local/ProcessMemoryDump
>>Do a memory dump for the selected Process

>- Auxiliary/Local/ProcessRun
>>Execute a system command in local machine

>- Auxiliary/Local/RSync
>>Remote sync for folder

>- Auxiliary/Local/Server/DnsServer
>>DNS Server

>- Auxiliary/Local/Server/SocksPortForwarding
>>Invisible socks port forwarding

>- Auxiliary/Local/Sniffer
>>Local Sniffer

>- Auxiliary/Local/Steganography/SteganographyImage
>>Steganography by Image generator/parser (in PNG)
>>Have two modes:
>>>              - Write: Destroy original message file
>>>              - Read : Read the image and write the secret file in LocalFileWrite

>- Auxiliary/Local/TestPayload
>>NFC Restore system

>- Auxiliary/Local/Tor
>>Tor Process

>- Auxiliary/Local/Windows/BinaryFromScreen
>> Binary from screen

>- Auxiliary/Local/Windows/KeyDown
>>Key down a textfile

>- Auxiliary/Local/Windows/WMIManager
>>WMI call

>- Auxiliary/Local/WordListBruteForce
>>Local Brute force by wordlist

>- Auxiliary/Local/WordListGenerator
>>Generate a wordList

>- Auxiliary/Multi/SSH/DownloadFile
>>Get a binay from SSH machine

>- Auxiliary/Multi/SSH/FastExecution
>>Execute SSH stream to exe machine

>- Auxiliary/Multi/SSH/PortForwarding
>>Port Forwarding from SSH machine

#### <i class="icon-target"></i> Exploits
Complete list of all exploits:

>- Exploits/Multi/Netcat/PrintFormat
>>Get a binay from SSH machine

>- Exploits/Multi/VulnServer
>>VulnServer exploit


#### <i class="icon-target"></i> Payloads
Complete list of all payloads:

>- Payloads/Local/BruteForce/BruteForceBitLockerAPI
>> Crack Bitlocker drive calling windows API


>- Payloads/Local/BruteForce/BruteForceBi
>> Crack Bitlocker drive

>- Payloads/Local/BruteForce/BruteForceMySQLWireshark
>>Crack MySql sniffed with WireShark Credentials

>- Payloads/Local/BruteForce/NFC/BruteForceNFCMifare
>>Mifare bruteforce

>- Payloads/Local/Fuzzing/TcpSocketFuzzer
>>Send fuzzer by TCP Socket

>- Payloads/Local/RSync/Ftp
>>Ftp rsync

>- Payloads/Local/RSync/LocalPath
>> Sync local path

>- Payloads/Local/Sniffer/DeepScan
>>Sniffer insecure protocols passwords

>- Payloads/Local/Sniffer/DumpToFolder
>>Sniffer to folder

>- Payloads/Local/Sniffer/TcpPacketInjection
>>Tcp Packet Injection

>- Payloads/Local/Windows/WMI/Action/ExecuteProcess
>>Execute a process in WMI

>- Payloads/Local/Windows/WMI/Query/Auto
>>Execute a default query in WMI

>- Payloads/Local/Windows/WMI/Query/Manual
>> Execute a default query in WMI

>- Payloads/Multi/Windows/x86/PayloadX86WindowsMessageBox
>>Show MessageBox
