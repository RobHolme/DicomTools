---
document type: cmdlet
external help file: DICOMtools.dll-Help.xml
HelpUri: ''
Locale: en-AU
Module Name: DicomTools
ms.date: 07/11/2026
PlatyPS schema version: 2024-05-01
title: Send-CMove
---

# Send-CMove

## SYNOPSIS

Send a DICOM C-MOVE to instruct a remote host to send images to a specified move destination AE title.

## SYNTAX

### Study

```
Send-CMove [-HostName] <string> [-Port] <int> [[-LocalAETitle] <string>] [[-RemoteAETitle] <string>]
 [-MoveDestination] <string> [-StudyInstanceUID] <string> [-UseTLS] [[-Timeout] <int>]
 [<CommonParameters>]
```

### Series

```
Send-CMove [-HostName] <string> [-Port] <int> [[-LocalAETitle] <string>] [[-RemoteAETitle] <string>]
 [-MoveDestination] <string> [-SeriesInstanceUID] <string> [-UseTLS] [[-Timeout] <int>]
 [<CommonParameters>]
```

### Image

```
Send-CMove [-HostName] <string> [-Port] <int> [[-LocalAETitle] <string>] [[-RemoteAETitle] <string>]
 [-MoveDestination] <string> [-SOPInstanceUID] <string> [-UseTLS] [[-Timeout] <int>]
 [<CommonParameters>]
```

## DESCRIPTION

Send a DICOM C-MOVE to instruct a remote host to send images to a specified move destination AE title.

## EXAMPLES

## PARAMETERS

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

### -MoveDestination

The AE title of the destination to send images to

```yaml
Type: System.String
DefaultValue: ''
SupportsWildcards: false
Aliases: []
ParameterSets:
- Name: (All)
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

### -SeriesInstanceUID

The Series Instance UID to retrieve

```yaml
Type: System.String
DefaultValue: ''
SupportsWildcards: false
Aliases: []
ParameterSets:
- Name: Series
  Position: 6
  IsRequired: true
  ValueFromPipeline: false
  ValueFromPipelineByPropertyName: false
  ValueFromRemainingArguments: false
DontShow: false
AcceptedValues: []
HelpMessage: ''
```

### -SOPInstanceUID

The SOP Instance UID to retrieve

```yaml
Type: System.String
DefaultValue: ''
SupportsWildcards: false
Aliases: []
ParameterSets:
- Name: Image
  Position: 6
  IsRequired: true
  ValueFromPipeline: false
  ValueFromPipelineByPropertyName: false
  ValueFromRemainingArguments: false
DontShow: false
AcceptedValues: []
HelpMessage: ''
```

### -StudyInstanceUID

The Study Instance UID to retrieve

```yaml
Type: System.String
DefaultValue: ''
SupportsWildcards: false
Aliases: []
ParameterSets:
- Name: Study
  Position: 6
  IsRequired: true
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
  Position: 8
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
  Position: 7
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

- [Project site](https://github.com/RobHolme/DicomTools#send-cmove)

