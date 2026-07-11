---
document type: cmdlet
external help file: DICOMtools.dll-Help.xml
HelpUri: https://github.com/RobHolme/DicomTools#send-cfind
Locale: en-AU
Module Name: DicomTools
ms.date: 07/11/2026
PlatyPS schema version: 2024-05-01
title: Send-CFind
---

# Send-CFind

## SYNOPSIS

Send a C-FIND query to a DICOM interface, display the results returned.

## SYNTAX

### PatientDemographics

```
Send-CFind [-HostName] <string> [-Port] <int> [[-LocalAETitle] <string>] [[-RemoteAETitle] <string>]
 [-PatientName] <string> [[-Modality] <string>] [[-StartDate] <string>] [[-EndDate] <string>]
 [-UseTLS] [[-Timeout] <int>] [<CommonParameters>]
```

### PatientID

```
Send-CFind [-HostName] <string> [-Port] <int> [[-LocalAETitle] <string>] [[-RemoteAETitle] <string>]
 [-PatientID] <string> [[-Modality] <string>] [[-StartDate] <string>] [[-EndDate] <string>]
 [-UseTLS] [[-Timeout] <int>] [<CommonParameters>]
```

### StudyID

```
Send-CFind [-HostName] <string> [-Port] <int> [[-LocalAETitle] <string>] [[-RemoteAETitle] <string>]
 [-StudyID] <string> [[-Modality] <string>] [[-StartDate] <string>] [[-EndDate] <string>] [-UseTLS]
 [[-Timeout] <int>] [<CommonParameters>]
```

### AccessionNumber

```
Send-CFind [-HostName] <string> [-Port] <int> [[-LocalAETitle] <string>] [[-RemoteAETitle] <string>]
 [-AccessionNumber] <string> [[-Modality] <string>] [[-StartDate] <string>] [[-EndDate] <string>]
 [-UseTLS] [[-Timeout] <int>] [<CommonParameters>]
```

## DESCRIPTION

Send a C-FIND query to a DICOM interface, display the results returned.
Default values for LocalAETitle and RemoteAETitle used if not supplied via the parameters.

Search by values provided in either PatientName, PatientID, StudyID, or AccessionNumber parameters.
Include * search string for wildcard searches.
Warning: A single search value of * will return all studies.

## EXAMPLES

### EXAMPLE 1

C:\PS> Send-CFind -HostName www.dicomserver.co.uk -Port 11112 -PatientName test

PatientName  PatientID  BirthDate Sex Modality StudyDate              AccessionNumber StudyInstanceID
-----------  ---------  --------- --- -------- ---------              --------------- ---------------
test^test    EkeaB142dw 20210708  M   DOC      15/07/2021 12:00:00 AM                 1.2.276.0.7230010.3.1.2.8323328.16910.16…
test^test    EkeaB142dw 20210708  M   DOC      15/07/2021 12:00:00 AM                 1.2.276.0.7230010.3.1.2.8323328.18104.16…
test^test    EkeaB142dw 20210708  M   DOC      15/07/2021 12:00:00 AM                 1.2.276.0.7230010.3.1.2.8323328.18673.16…
test         3          20210616  O   SC       13/01/2021 10:13:51 AM                 2.25.12671083270627443311864223603381287…
test         3          20210616  O   SC       13/01/2021 10:13:51 AM                 2.25.22697510870666633359788882595239852…
test         3          20210616  O   SC       17/07/2021 12:20:25 PM                 2.25.15048971584590926188919414102281229…
Test^Patient 0          19600709  F   OAM      9/07/2020 11:31:11 AM                  1.2.276.0.75.2.1.11.1.1.200709113729721.…

Find all patients where Patient Name is 'Test'

### EXAMPLE 2

C:\PS> Send-CFind -HostName www.dicomserver.co.uk -Port 11112 -PatientID 123*

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

Find all patients with Patient ID starting with '123'

### EXAMPLE 3

C:\PS> Send-CFind -HostName www.dicomserver.co.uk -Port 11112 -PatientName * -Modality CT -startDate 'Jan 1 2020' -EndDate '31/12/2020' 

PatientName PatientID BirthDate Sex Modality StudyDate             AccessionNumber StudyInstanceID
----------- --------- --------- --- -------- ---------             --------------- ---------------
.           139091                  CT       28/01/2020 5:29:09 PM                 1.3.6.1.4.1.5962.99.1.822597906.190419455.1…
.           139091                  CT       28/01/2020 5:29:09 PM                 1.3.6.1.4.1.5962.99.1.831748304.238282405.1…
.           139091                  CT       5/02/2020 2:47:03 PM                  1.3.6.1.4.1.5962.99.1.822597906.190419455.1…
.           139091                  CT       13/02/2020 9:08:15 AM                 1.3.6.1.4.1.5962.99.1.822597906.190419455.1…
Anonymous   Anonymous               CT       9/12/2020 8:34:04 PM                  2.16.840.1.114540.200114023.27924

Find all CT scans acquired from 1 Jan 2020 through to 31 Dec 2020

### EXAMPLE 4

C:\PS> Send-CFind -HostName www.dicomserver.co.uk -Port 11112 -StudyID 1.2.826.0.1.3680043.2.891.113.20210702141058064482710

PatientName PatientID BirthDate Sex Modality StudyDate            AccessionNumber StudyInstanceID
----------- --------- --------- --- -------- ---------            --------------- ---------------
Tester^Test 12345     19850202  M   ES       2/07/2021 2:10:58 PM 7654321         1.2.826.0.1.3680043.2.891.113.20210702141058064482710

Find patient details for Study Instance ID of '1.2.826.0.1.3680043.2.891.113.20210702141058064482710'

## PARAMETERS

### -AccessionNumber

The Accession number to search for.
Can include '*' for wildcard searches.

```yaml
Type: System.String
DefaultValue: ''
SupportsWildcards: false
Aliases: []
ParameterSets:
- Name: AccessionNumber
  Position: 5
  IsRequired: true
  ValueFromPipeline: false
  ValueFromPipelineByPropertyName: false
  ValueFromRemainingArguments: false
DontShow: false
AcceptedValues: []
HelpMessage: ''
```

### -EndDate

Constrain the search for studies acquired on or before this date.
Accepts any string that can be parsed as a date.

```yaml
Type: System.String
DefaultValue: ''
SupportsWildcards: false
Aliases: []
ParameterSets:
- Name: (All)
  Position: 8
  IsRequired: false
  ValueFromPipeline: false
  ValueFromPipelineByPropertyName: false
  ValueFromRemainingArguments: false
DontShow: false
AcceptedValues: []
HelpMessage: ''
```

### -HostName

Hostname or IP Address of DICOM service

```yaml
Type: System.String
DefaultValue: ''
SupportsWildcards: false
Aliases:
- IPAddress
ParameterSets:
- Name: (All)
  Position: 1
  IsRequired: true
  ValueFromPipeline: true
  ValueFromPipelineByPropertyName: true
  ValueFromRemainingArguments: false
DontShow: false
AcceptedValues: []
HelpMessage: ''
```

### -LocalAETitle

The local (calling) AE Title

```yaml
Type: System.String
DefaultValue: DICOMTOOLS-SCU
SupportsWildcards: false
Aliases:
- CallingAETitle
ParameterSets:
- Name: (All)
  Position: 3
  IsRequired: false
  ValueFromPipeline: false
  ValueFromPipelineByPropertyName: false
  ValueFromRemainingArguments: false
DontShow: false
AcceptedValues: []
HelpMessage: ''
```

### -Modality

Constrain the search to a modality type.

```yaml
Type: System.String
DefaultValue: ''
SupportsWildcards: false
Aliases: []
ParameterSets:
- Name: (All)
  Position: 6
  IsRequired: false
  ValueFromPipeline: false
  ValueFromPipelineByPropertyName: false
  ValueFromRemainingArguments: false
DontShow: false
AcceptedValues: []
HelpMessage: ''
```

### -PatientID

The patient ID to search for.
Can include '*' for wildcard searches.

```yaml
Type: System.String
DefaultValue: ''
SupportsWildcards: false
Aliases: []
ParameterSets:
- Name: PatientID
  Position: 5
  IsRequired: true
  ValueFromPipeline: false
  ValueFromPipelineByPropertyName: false
  ValueFromRemainingArguments: false
DontShow: false
AcceptedValues: []
HelpMessage: ''
```

### -PatientName

The name of the patient to search for.
Can include '*' for wildcard searches.

```yaml
Type: System.String
DefaultValue: ''
SupportsWildcards: false
Aliases: []
ParameterSets:
- Name: PatientDemographics
  Position: 5
  IsRequired: true
  ValueFromPipeline: false
  ValueFromPipelineByPropertyName: false
  ValueFromRemainingArguments: false
DontShow: false
AcceptedValues: []
HelpMessage: ''
```

### -Port

The port number of the remote DICOM interface.

```yaml
Type: System.Int32
DefaultValue: ''
SupportsWildcards: false
Aliases: []
ParameterSets:
- Name: (All)
  Position: 2
  IsRequired: true
  ValueFromPipeline: false
  ValueFromPipelineByPropertyName: false
  ValueFromRemainingArguments: false
DontShow: false
AcceptedValues: []
HelpMessage: ''
```

### -RemoteAETitle

The remote (called) AE Title

```yaml
Type: System.String
DefaultValue: ANY-SCP
SupportsWildcards: false
Aliases:
- CalledAETitle
ParameterSets:
- Name: (All)
  Position: 4
  IsRequired: false
  ValueFromPipeline: false
  ValueFromPipelineByPropertyName: false
  ValueFromRemainingArguments: false
DontShow: false
AcceptedValues: []
HelpMessage: ''
```

### -StartDate

Constrain the search for studies acquired on or after this date.
Accepts any string that can be parsed as a date.

```yaml
Type: System.String
DefaultValue: ''
SupportsWildcards: false
Aliases: []
ParameterSets:
- Name: (All)
  Position: 7
  IsRequired: false
  ValueFromPipeline: false
  ValueFromPipelineByPropertyName: false
  ValueFromRemainingArguments: false
DontShow: false
AcceptedValues: []
HelpMessage: ''
```

### -StudyID

The study instance ID to search for.
Can include '*' for wildcard searches.

```yaml
Type: System.String
DefaultValue: ''
SupportsWildcards: false
Aliases: []
ParameterSets:
- Name: StudyID
  Position: 5
  IsRequired: true
  ValueFromPipeline: false
  ValueFromPipelineByPropertyName: false
  ValueFromRemainingArguments: false
DontShow: false
AcceptedValues: []
HelpMessage: ''
```

### -Timeout

The timeout in seconds to wait for DICOM association, or a response from the DICOM service once a request has been submitted.

```yaml
Type: System.Int32
DefaultValue: 20
SupportsWildcards: false
Aliases: []
ParameterSets:
- Name: (All)
  Position: 10
  IsRequired: false
  ValueFromPipeline: false
  ValueFromPipelineByPropertyName: false
  ValueFromRemainingArguments: false
DontShow: false
AcceptedValues: []
HelpMessage: ''
```

### -UseTLS

Negiotiate a TLS secured connection (the remote DICOM interface must support TLS secured connections).

```yaml
Type: System.Management.Automation.SwitchParameter
DefaultValue: $false
SupportsWildcards: false
Aliases: []
ParameterSets:
- Name: (All)
  Position: 9
  IsRequired: false
  ValueFromPipeline: false
  ValueFromPipelineByPropertyName: false
  ValueFromRemainingArguments: false
DontShow: false
AcceptedValues: []
HelpMessage: ''
```

### CommonParameters

This cmdlet supports the common parameters: -Debug, -ErrorAction, -ErrorVariable,
-InformationAction, -InformationVariable, -OutBuffer, -OutVariable, -PipelineVariable,
-ProgressAction, -Verbose, -WarningAction, and -WarningVariable. For more information, see
[about_CommonParameters](https://go.microsoft.com/fwlink/?LinkID=113216).


## RELATED LINKS

- [Project site](https://github.com/RobHolme/DicomTools#send-cfind)
