---
document type: cmdlet
external help file: DICOMtools.dll-Help.xml
HelpUri: ''
Locale: en-AU
Module Name: DicomTools
ms.date: 07/11/2026
PlatyPS schema version: 2024-05-01
title: Send-DMWLQuery
---

# Send-DMWLQuery

## SYNOPSIS

Query a DICOM Modality Work List (DMWL), display the results returned.

## SYNTAX

### __AllParameterSets

```
Send-DMWLQuery [-HostName] <string> [-Port] <int> [[-LocalAETitle] <string>]
 [[-RemoteAETitle] <string>] [[-PatientName] <string>] [[-PatientID] <string>]
 [[-Modality] <string>] [[-StartDateTime] <string>] [[-EndDateTime] <string>] [-UseTLS]
 [[-Timeout] <int>] [<CommonParameters>]
```

## ALIASES

This cmdlet has the following aliases,
  {{Insert list of aliases}}

## DESCRIPTION

Query a DICOM Modality Work List (DMWL), display the results returned. Default values for LocalAETitle and RemoteAETitle used if not supplied via the parameters. The default results will contain Patient details, to access scheduled procedure steps expand the 'ProcedureSteps' property returned. 

Restrict search by PatientName, PatientID, or Modality parameters. Include * search string for wildcard searches. Warning: A single search value of * will return all studies.

The StartDateTime and EndDateTime parameters can be supplied to constrain the search to exams scheduled between these dates. Both parameters must be supplied to create a date range. Any text value can be entered providing it can be recognised as a Date or DateTime value. e.g. "1997-09-18 20:00" or "18 Sept 97 8pm" would be accepted.

## EXAMPLES

### Example 1

Query DICOM modality worklist:

PS> Send-DMWLQuery -HostName www.dicomserver.co.uk -Port 11112

PatientName           PatientID BirthDate Sex Steps Modality  AccessionNumber StudyDescription  ProcedureSteps
-----------           --------- --------- --- ----- --------  --------------- ----------------  --------------
Bowen^William^^Dr     PAT004    19560807  M   1     MR        125             CT Left Shoulder  {1}
Bloggs^Joe^^Mr        PAT001    19450703  M   1     CT        123             CT Brain          {1}
Williams^Jane^^Mrs    PAT002    19830806  F   1     MR        124             MRI Left Shoulder {1}
Smith^Emma^^Miss      PAT003    19480603  F   1     RF        126             Left Leg DSA      {1}
Bowen^William^^Dr     PAT004    19560807  M   1     CT        125             MRI Left Shoulder {1}
Bowen^William^^Dr     PAT004    19560807  M   1     US        125             US Left Shoulder  {1}

### Example 2

Query DICOM modality worklist, display the procedure steps for the 3rd result returned:

PS> (Send-DMWLQuery -HostName www.dicomserver.co.uk -Port 11112)[2].procedureSteps

StepID              : 1
Modality            : MR
PerformingPhysician : Evans^^^Dr
StepDateTime        : 1/01/2001 12:30:00 PM
StepDescription     : MRI Left Shoulder

### Example 3

Query patients with a scheduled CT exam between 4pm and 8pm on 18 September 1997:

PS> Send-DMWLQuery  -HostName www.dicomserver.co.uk -Port 11112 -StartDateTime "Sept 18 1997 1pm" -EndDateTime "1997-09-18 20:00"  -Modality CT

PatientName       PatientID BirthDate Sex Steps Modality AccessionNumber StudyDescription  ProcedureSteps
-----------       --------- --------- --- ----- -------- --------------- ----------------  --------------
Bowen^William^^Dr PAT004    19560807  M   1     MR       125             CT Left Shoulder  {1}
Bloggs^Joe^^Mr    PAT001    19450703  M   1     CT       123             CT Brain          {1}
Bowen^William^^Dr PAT004    19560807  M   1     CT       125             MRI Left Shoulder {1}


## PARAMETERS

### -EndDateTime

Include studies from or after this date YYYYMMDD

```yaml
Type: System.String
DefaultValue: ''
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

The client calling AE title

```yaml
Type: System.String
DefaultValue: ''
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

Constrain results to specific modalities

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

### -PatientID

Include studies scheduled for this Patient ID

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

### -PatientName

Include studies scheduled for this Patient name

```yaml
Type: System.String
DefaultValue: ''
SupportsWildcards: false
Aliases: []
ParameterSets:
- Name: (All)
  Position: 5
  IsRequired: false
  ValueFromPipeline: false
  ValueFromPipelineByPropertyName: false
  ValueFromRemainingArguments: false
DontShow: false
AcceptedValues: []
HelpMessage: ''
```

### -Port

Port number of remote DICOM service

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

The server called AE title

```yaml
Type: System.String
DefaultValue: ''
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

### -StartDateTime

Include studies scheduled from or after this date (and time)

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

### -Timeout

The timeout in seconds to wait for a response

```yaml
Type: System.Int32
DefaultValue: ''
SupportsWildcards: false
Aliases: []
ParameterSets:
- Name: (All)
  Position: 11
  IsRequired: false
  ValueFromPipeline: false
  ValueFromPipelineByPropertyName: false
  ValueFromRemainingArguments: false
DontShow: false
AcceptedValues: []
HelpMessage: ''
```

### -UseTLS

Use TLS to secure the connection

```yaml
Type: System.Management.Automation.SwitchParameter
DefaultValue: ''
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

### CommonParameters

This cmdlet supports the common parameters: -Debug, -ErrorAction, -ErrorVariable,
-InformationAction, -InformationVariable, -OutBuffer, -OutVariable, -PipelineVariable,
-ProgressAction, -Verbose, -WarningAction, and -WarningVariable. For more information, see
[about_CommonParameters](https://go.microsoft.com/fwlink/?LinkID=113216).

## RELATED LINKS

- [Project Site](https://github.com/RobHolme/DicomTools#send-dwmlquery)

