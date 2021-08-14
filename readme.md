# Project Description
This PowerShell module is a small collection of CmdLets to test DICOM interfaces:

* __Send-CEcho__: send a DICOM C-ECHO to a DICOM endpoint
* __Send-CFind__: send a DICOM C-Find request to a DICOM endpoint
* __Show-DicomHeaders__: list the DICOM headers from a DICOM file

For Powershell Core v7+ only, this module does not support Windows Powershell.


# CmdLet Usage 
## __Send-CEcho
```Powershell
Send-CEcho [-HostName] <string> [-Port] <int> [[-LocalAETitle] <string>] [[-RemoteAETitle] <string>] [[-UseTLS]] [<CommonParameters>]
```
### Parameters
__-HostName <string>__

__-Port <int>__

__-LocalAETitle <string>__ 

__-RemoteAETitle <string>__

__-UseTLS__


## __Send-CFind


## __Show-DicomHeaders
