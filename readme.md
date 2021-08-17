# Project Description
This PowerShell module is a small collection of CmdLets to test DICOM interfaces:

* __Send-CEcho__: send a DICOM C-ECHO to a DICOM endpoint.
* __Send-CFind__: send a DICOM C-Find request to a DICOM endpoint, display the results returned.
* __Get-DicomTag__: list the DICOM tags from a DICOM file.

For Powershell Core v7+ only, this module does not support Windows PowerShell.

This module is a wrapper for the Fellow OAK DICOM (fo-dicom) library. https://github.com/fo-dicom/fo-dicom

# CmdLet Usage 

## Send-CEcho
Send a C-ECHO to a DICOM interface. Default values for LocalAETitle and RemoteAETitle used if not supplied via the parameters.
```Powershell
Send-CEcho [-HostName] <string> [-Port] <int> [[-LocalAETitle] <string>] [[-RemoteAETitle] <string>] [[-UseTLS]] [<CommonParameters>]
```
### Parameters
__-HostName <string>__

__-Port <int>__

__-LocalAETitle <string>__ 

__-RemoteAETitle <string>__

__-Timeout <int>__

__-UseTLS__



## Send-CFind
Send a C-FIND query to a DICOM interface, display the results returned. Default values for LocalAETitle and RemoteAETitle used if not supplied via the parameters.

Search by values provided in either PatientName or PatientName parameters. Include * in Patient Name or ID for wildcard searches. Warning: A single search value of * will return all studies.

```Powershell
Send-CFind [-HostName] <string> [-Port] <int> [[-LocalAETitle] <string>] [-PatientName] <string> [[-UseTLS]] [<CommonParameters>]

Send-CFind [-HostName] <string> [-Port] <int> [[-LocalAETitle] <string>] [-PatientID] <string> [[-UseTLS]] [<CommonParameters>]
```


### Parameters
__-HostName <string>__

__-Port <int>__

__-LocalAETitle <string>__ 

__-RemoteAETitle <string>__

__-PatientName <string>__

__-PatientID <string>__

__-UseTLS__

### Examples
Find all patients where Patient Name is 'Test'
```
PS>Send-CFind -HostName www.dicomserver.co.uk -Port 11112 -PatientName test

PatientName  PatientID  PatientBirthDate PatientSex PatientAddress StudyDate StudyInstanceID
-----------  ---------  ---------------- ---------- -------------- --------- ---------------
test^test    EkeaB142dw 20210708         M                         20210715  1.2.276.0.7230010.3.1.2.8323328.16910.1626360000.332093
test^test    EkeaB142dw 20210708         M                         20210715  1.2.276.0.7230010.3.1.2.8323328.18104.1626360306.802249
test^test    EkeaB142dw 20210708         M                         20210715  1.2.276.0.7230010.3.1.2.8323328.18673.1626360435.834589
test         3          20210616         O                         20210113  2.25.126710832706274433118642236033812875497
test         3          20210616         O                         20210113  2.25.226975108706666333597888825952398527497
test         3          20210616         O                         20210717  2.25.150489715845909261889194141022812290825
Test^Patient 0          19600709         F                         20200709  1.2.276.0.75.2.1.11.1.1.200709113729721.966169828020.1000004
```

Find all patients with Patient ID starting with '123'
```
PS>Send-CFind -HostName www.dicomserver.co.uk -Port 11112 -PatientID 123*

PatientName PatientID  PatientBirthDate PatientSex PatientAddress StudyDate StudyInstanceID
----------- ---------  ---------------- ---------- -------------- --------- ---------------
*001        1234567890 20150511         F                         20150511  1.2.826.0.1.3680043.6.98861.89036.20150511111711.4680.1.2
Tester^Test 12345      19850202         M                         20210702  1.2.826.0.1.3680043.2.891.113.20210702140306987482710
Tester^Test 12345      19850202         M                         20210702  1.2.826.0.1.3680043.2.891.113.202107021403125678547165057
Tester^Test 12345      19850202         M                         20210702  1.2.826.0.1.3680043.2.891.113.20210702140314031120342820711
Tester^Test 12345      19850202         M                         20210702  1.2.826.0.1.3680043.2.891.113.20210702140315525109889433915
Tester^Test 12345      19850202         M                         20210702  1.2.826.0.1.3680043.2.891.113.20210702140316943207578209519
Tester^Test 12345      19850202         M                         20210702  1.2.826.0.1.3680043.2.891.113.20210702140318472140230408723
Tester^Test 12345      19850202         M                         20210702  1.2.826.0.1.3680043.2.891.113.20210702141058064482710
Tester^Test 12345      19850202         M                         20210719  1.2.826.0.1.3680043.2.891.113.202107190934534104365267479
```

## Get-DicomTag
Displays the DICOM tags from a DICOM file. 

```Powershell
Get-DicomTag -LiteralPath <string[]> [<CommonParameters>]

Get-DicomTag [-Path] <string[]> [<CommonParameters>]
```