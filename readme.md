# Project Description
This PowerShell module is a small collection of CmdLets to test DICOM interfaces:

* __Send-CEcho__: send a DICOM C-ECHO to a DICOM endpoint.
* __Send-CFind__: send a DICOM C-Find request to a DICOM endpoint, display the results returned.
* __Get-DicomTag__: list the DICOM tags from a DICOM file.

For Powershell Core v7+ only, this module does not support Windows PowerShell.

This module uses the Fellow OAK DICOM (fo-dicom) library. https://github.com/fo-dicom/fo-dicom

# Installation 
Either build the solution (instructions below), or download the latest release from https://github.com/RobHolme/DicomTools/releases. 

Copy the contents of the /module folder (or extract the files from the release) to a folder named 'DicomTools' in the Powershell module path (use $env:PSModulePath to list all paths). 

Requires PowerShell 7.0 or greater, Windows Powershell is not supported.
## Build instructions
Install the .Net Core 3.1 SDK (https://dotnet.microsoft.com/download/visual-studio-sdks). 

Edit the SDK version in global.json. 

Run ```dotnet build --configuration Release```

Copy the built DicomTools.dll to .\module\lib.


# CmdLet Usage 

## Send-CEcho
Send a C-ECHO to a DICOM interface. Default values for LocalAETitle and RemoteAETitle used if not supplied via the parameters.
```Powershell
Send-CEcho [-HostName] <string> [-Port] <int> [[-LocalAETitle] <string>] [[-RemoteAETitle] <string>] [[-UseTLS]] [<CommonParameters>]
```
### Parameters
__-HostName <string>__ The hostname of the DICOM interface to query.

__-Port <int>__ The TCP port number of the DICOM interface.

__-LocalAETitle <string>__  The caller AE title. Defaults to 'DICOMTOOLS-SCU' if no parameter supplied.

__-RemoteAETitle <string>__ The called AE title. Defaults to 'ANY-SCP' if no parameter supplied.

__-Timeout <int>__ The timout in seconds before the DICOM association is cancelled (or time to wait for a response to the C-ECHO if the association was successful).

__-UseTLS__ Use TLS to secure the connection (if supported by the remote DICOM service).

### Example
```
PS>Send-CEcho -HostName www.dicomserver.co.uk -Port 11112

Hostname              Port  ResponseTime Status
--------              ----  ------------ ------
www.dicomserver.co.uk 11112 1587 ms      Success
```


## Send-CFind
Send a C-FIND query to a DICOM interface, display the results returned. Default values for LocalAETitle and RemoteAETitle used if not supplied via the parameters.

Search by values provided in either PatientName, PatientID, or StudyID parameters. Include * search string for wildcard searches. Warning: A single search value of * will return all studies.

```Powershell
Send-CFind [-HostName] <string> [-Port] <int> [[-LocalAETitle] <string>] [[-RemoteAETitle] <string>] [-PatientName] <string> [[-Modality] <string>] [[-StartDate] <string>] [[-EndDate] <string>] [[-UseTLS]] [[-Timeout] <int>] [<CommonParameters>]

Send-CFind [-HostName] <string> [-Port] <int> [[-LocalAETitle] <string>] [[-RemoteAETitle] <string>] [-PatientID] <string> [[-Modality] <string>] [[-StartDate] <string>] [[-EndDate] <string>] [[-UseTLS]] [[-Timeout] <int>] [<CommonParameters>]

Send-CFind [-HostName] <string> [-Port] <int> [[-LocalAETitle] <string>] [[-RemoteAETitle] <string>] [-StudyID] <string> [[-Modality] <string>] [[-StartDate] <string>] [[-EndDate] <string>] [[-UseTLS]] [[-Timeout] <int>] [<CommonParameters>]

Send-CFind [-HostName] <string> [-Port] <int> [[-LocalAETitle] <string>] [[-RemoteAETitle] <string>] [-AccessionNumber] <string> [[-Modality] <string>] [[-StartDate] <string>] [[-EndDate] <string>] [[-UseTLS]] [[-Timeout] <int>] [<CommonParameters>]
```

### Parameters
__-HostName <string>__ The hostname of the DICOM interface to query.

__-Port <int>__ The TCP port number of the DICOM interface.

__-LocalAETitle <string>__  The caller AE title. Defaults to 'DICOMTOOLS-SCU' if no parameter supplied.

__-RemoteAETitle <string>__ The called AE title. Defaults to 'ANY-SCP' if no parameter supplied.

__-PatientName <string>__ The name of the patient to search for. Can include '*' for wildcard searches.

__-PatientID <string>__ The patient ID to search for. Can include '*' for wildcard searches.

__-StudyID <string>__ The study instance ID to search for. Can include '*' for wildcard searches.

__-AccessionNumber <string>__ The Accession number to search for. Can include '*' for wildcard searches.

__-Modality <string>__ Constrain the search to a modality type.

__-StartDate <string>__ Constrain the search for studies acquired on or after this date. Accepts any string that can be parsed as a date.

__-EndDate <string>__ Constrain the search for studies acquired on or before this date. Accepts any string that can be parsed as a date.

__-UseTLS__ Use TLS to secure the connection (if supported by the remote DICOM service).

__-Timeout <int>__ The timout in seconds before the DICOM association is cancelled (or time to wait for a response to the C-FIND if association was successful).

### Examples
Find all patients where Patient Name is 'Test'
```
PS>Send-CFind -HostName www.dicomserver.co.uk -Port 11112 -PatientName test

PatientName  PatientID  BirthDate Sex Modality StudyDate              AccessionNumber StudyInstanceID
-----------  ---------  --------- --- -------- ---------              --------------- ---------------
test^test    EkeaB142dw 20210708  M   DOC      15/07/2021 12:00:00 AM                 1.2.276.0.7230010.3.1.2.8323328.16910.16…
test^test    EkeaB142dw 20210708  M   DOC      15/07/2021 12:00:00 AM                 1.2.276.0.7230010.3.1.2.8323328.18104.16…
test^test    EkeaB142dw 20210708  M   DOC      15/07/2021 12:00:00 AM                 1.2.276.0.7230010.3.1.2.8323328.18673.16…
test         3          20210616  O   SC       13/01/2021 10:13:51 AM                 2.25.12671083270627443311864223603381287…
test         3          20210616  O   SC       13/01/2021 10:13:51 AM                 2.25.22697510870666633359788882595239852…
test         3          20210616  O   SC       17/07/2021 12:20:25 PM                 2.25.15048971584590926188919414102281229…
Test^Patient 0          19600709  F   OAM      9/07/2020 11:31:11 AM                  1.2.276.0.75.2.1.11.1.1.200709113729721.…
```

Find all patients with Patient ID starting with '123'
```
PS>Send-CFind -HostName www.dicomserver.co.uk -Port 11112 -PatientID 123*

PatientName    PatientID  BirthDate Sex Modality StudyDate             AccessionNumber StudyInstanceID
-----------    ---------  --------- --- -------- ---------             --------------- ---------------
*001           1234567890 20150511  F   CR       11/05/2015 4:26:38 AM 19035189        1.2.826.0.1.3680043.6.98861.89036.20150…
DEMO_RECORDING 123456                   XA       24/08/2021 3:22:41 PM                 2.25.1788281389745095966601891702358043…
Tester^Test    12345      19850202  M   ES       2/07/2021 2:03:06 PM                  1.2.826.0.1.3680043.2.891.113.202107021…
Tester^Test    12345      19850202  M   ES       2/07/2021 2:03:12 PM                  1.2.826.0.1.3680043.2.891.113.202107021…
Tester^Test    12345      19850202  M   ES       2/07/2021 2:03:14 PM                  1.2.826.0.1.3680043.2.891.113.202107021…
Tester^Test    12345      19850202  M   ES       2/07/2021 2:03:15 PM                  1.2.826.0.1.3680043.2.891.113.202107021…
Tester^Test    12345      19850202  M   ES       2/07/2021 2:03:16 PM                  1.2.826.0.1.3680043.2.891.113.202107021…
Tester^Test    12345      19850202  M   ES       2/07/2021 2:03:18 PM                  1.2.826.0.1.3680043.2.891.113.202107021…
Tester^Test    12345      19850202  M   ES       2/07/2021 2:10:58 PM  7654321         1.2.826.0.1.3680043.2.891.113.202107021…
Tester^Test    12345      19850202  M   ES       19/07/2021 9:34:53 AM                 1.2.826.0.1.3680043.2.891.113.202107190…
```

Find all CT scans acquired from 1 Jan 2020 through to 31 Dec 2020
```
PS>Send-CFind -HostName www.dicomserver.co.uk -Port 11112 -PatientName * -Modality CT -startDate 'Jan 1 2020' -EndDate '31/12/2020' 

PatientName PatientID BirthDate Sex Modality StudyDate             AccessionNumber StudyInstanceID
----------- --------- --------- --- -------- ---------             --------------- ---------------
.           139091                  CT       28/01/2020 5:29:09 PM                 1.3.6.1.4.1.5962.99.1.822597906.190419455.1…
.           139091                  CT       28/01/2020 5:29:09 PM                 1.3.6.1.4.1.5962.99.1.831748304.238282405.1…
.           139091                  CT       5/02/2020 2:47:03 PM                  1.3.6.1.4.1.5962.99.1.822597906.190419455.1…
.           139091                  CT       13/02/2020 9:08:15 AM                 1.3.6.1.4.1.5962.99.1.822597906.190419455.1…
Anonymous   Anonymous               CT       9/12/2020 8:34:04 PM                  2.16.840.1.114540.200114023.27924
```

Find patient details for Study Instance ID of '1.2.826.0.1.3680043.2.891.113.20210702141058064482710'
```
PS>Send-CFind -HostName www.dicomserver.co.uk -Port 11112 -StudyID 1.2.826.0.1.3680043.2.891.113.20210702141058064482710

PatientName PatientID BirthDate Sex Modality StudyDate            AccessionNumber StudyInstanceID
----------- --------- --------- --- -------- ---------            --------------- ---------------
Tester^Test 12345     19850202  M   ES       2/07/2021 2:10:58 PM 7654321         1.2.826.0.1.3680043.2.891.113.20210702141058064482710
```


## Get-DicomTag
Displays the DICOM tags from a DICOM file. 

```Powershell
Get-DicomTag -LiteralPath <string[]> [<CommonParameters>]

Get-DicomTag [-Path] <string[]> [<CommonParameters>]
```

### Parameters
__-LiteralPath <string>__ The literal path to the DICOM file.

__-Path <string>__ The path of the DICOM file(s). Can include wildcards or regular expressions to specify multiple files.  
