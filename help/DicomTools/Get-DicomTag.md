---
document type: cmdlet
external help file: DICOMtools.dll-Help.xml
HelpUri: https://github.com/RobHolme/DicomTools#get-dicomtag
Locale: en-AU
Module Name: DicomTools
ms.date: 07/11/2026
PlatyPS schema version: 2024-05-01
title: Get-DicomTag
---

# Get-DicomTag

## SYNOPSIS

Displays the DICOM tags from a DICOM file.

## SYNTAX

### Literal

```
Get-DicomTag -LiteralPath <string[]> [<CommonParameters>]
```

### Path

```
Get-DicomTag [-Path] <string[]> [<CommonParameters>]
```

## DESCRIPTION

Displays the DICOM tags from a DICOM file.

## PARAMETERS

### -LiteralPath

The literal path to a DICOM file.

```yaml
Type: System.String[]
DefaultValue: ''
SupportsWildcards: false
Aliases:
- PSPath
- Filename
ParameterSets:
- Name: Literal
  Position: Named
  IsRequired: true
  ValueFromPipeline: false
  ValueFromPipelineByPropertyName: true
  ValueFromRemainingArguments: false
DontShow: false
AcceptedValues: []
HelpMessage: ''
```

### -Path

The path of the DICOM file(s).
Can include wildcards or regular expressions to specify multiple files.

```yaml
Type: System.String[]
DefaultValue: ''
SupportsWildcards: false
Aliases: []
ParameterSets:
- Name: Path
  Position: 0
  IsRequired: true
  ValueFromPipeline: true
  ValueFromPipelineByPropertyName: true
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

- [Project site](https://github.com/RobHolme/DicomTools#get-dicomtag)
