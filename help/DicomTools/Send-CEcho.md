---
document type: cmdlet
external help file: DICOMtools.dll-Help.xml
HelpUri: https://github.com/RobHolme/DicomTools#send-cecho
Locale: en-AU
Module Name: DicomTools
ms.date: 07/11/2026
PlatyPS schema version: 2024-05-01
title: Send-CEcho
---

# Send-CEcho

## SYNOPSIS

Sends a DICOM C-Echo to a DICOM endpoint.

## SYNTAX

### __AllParameterSets

```
Send-CEcho [-HostName] <string> [-Port] <int> [[-LocalAETitle] <string>] [[-RemoteAETitle] <string>]
 [-UseTLS] [[-Timeout] <int>] [<CommonParameters>]
```

## ALIASES

This cmdlet has the following aliases,
  {{Insert list of aliases}}

## DESCRIPTION

Sends a DICOM C-Echo to a DICOM endpoint.
Reteurn the response, and time taken to respond (If successfull).

## EXAMPLES

### Example 1

C:\PS> Send-CEcho -HostName www.dicomserver.co.uk -Port 11112

Hostname              Port  ResponseTime Status
--------              ----  ------------ ------
www.dicomserver.co.uk 11112 1587 ms      Success

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

### -Timeout

The timeout in seconds to wait for DICOM association, or a response from the DICOM service once a request has been submitted.

```yaml
Type: System.Int32
DefaultValue: 5
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

### -UseTLS

Negiotiate a TLS secured connection (the remote DICOM interface must support TLS secured connections).

```yaml
Type: System.Management.Automation.SwitchParameter
DefaultValue: $false
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

### CommonParameters

This cmdlet supports the common parameters: -Debug, -ErrorAction, -ErrorVariable,
-InformationAction, -InformationVariable, -OutBuffer, -OutVariable, -PipelineVariable,
-ProgressAction, -Verbose, -WarningAction, and -WarningVariable. For more information, see
[about_CommonParameters](https://go.microsoft.com/fwlink/?LinkID=113216).

## INPUTS

### System.String

{{ Fill in the Description }}

## OUTPUTS

## NOTES

## RELATED LINKS

- [Project site](https://github.com/RobHolme/DicomTools#send-cecho)
