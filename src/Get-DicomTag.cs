/* Filename:    GetDicomTag.cs
 * 
 * Author:      Rob Holme (rob@holme.com.au) 
 *              
 * Credits:     Code to handle the Path and LiteralPath parameter sets, and expansion of wildcards is based
 *              on Oisin Grehan's post: http://www.nivot.org/blog/post/2008/11/19/Quickstart1ACmdletThatProcessesFilesAndDirectories
 * 
 * Date:        15/08/2021
 * 
 * Notes:       Dislays all DicomTags from a DICOM file
 * 
 */
 	

namespace DicomTools
{
	using System;
	using System.Collections;
   	using System.IO;
	using System.Management.Automation;
	using Microsoft.PowerShell.Commands;
	using System.Collections.Generic;
	using Dicom;
	 
	// CmdLet: Get-DicomTag
    // Returns all DICOM tags from a DICOM file
    [Cmdlet(VerbsCommon.Get, "DicomTag")]
    public class GetDicomTag : PSCmdlet
    {
        private string[] paths;
        private bool expandWildcards = false;
       
        // Parameter set for the -Path and -LiteralPath parameters. A parameter set ensures these options are mutually exclusive.
        // A LiteralPath is used in situations where the filename actually contains wild card characters (eg File[1-10].txt) and you want
        // to use the literaral file name instead of treating it as a wildcard search.
        [Parameter(
            Mandatory = true,
            ValueFromPipeline = false,
            ValueFromPipelineByPropertyName = true,
            ParameterSetName = "Literal")
        ]
        [Alias("PSPath", "Filename")]
        [ValidateNotNullOrEmpty]
        public string[] LiteralPath
        {
            get { return this.paths; }
            set { this.paths = value; }
        }

        [Parameter(
            Position = 0,
            Mandatory = true,
            ValueFromPipeline = true,
            ValueFromPipelineByPropertyName = true,
            ParameterSetName = "Path")

        ]
        [ValidateNotNullOrEmpty]
        public string[] Path
        {
            get { return this.paths; }
            set
            {
                this.expandWildcards = true;
                this.paths = value;
            }
        }

        /// <summary>
        /// get the HL7 item provided via the cmdlet parameter HL7ItemPosition
        /// </summary>
        protected override void ProcessRecord()
        {

            // expand the file or directory information provided in the -Path or -LiteralPath parameters
            foreach (string path in paths) {
                // This will hold information about the provider containing the items that this path string might resolve to.                
                ProviderInfo provider;
                // This will be used by the method that processes literal paths
                PSDriveInfo drive;
                // this contains the paths to process for this iteration of the loop to resolve and optionally expand wildcards.
                List<string> filePaths = new List<string>();

                // if the path provided is a directory, expand the files in the directory and add these to the list.
                if (Directory.Exists(path)) {
                    filePaths.AddRange(Directory.GetFiles(path));
                }

                // not a directory, could be a wildcard or literal filepath 
                else {
                    // expand wildcards. This assumes if the user listed a directory it is literal
                    if (expandWildcards) {
                        // Turn *.txt into foo.txt,foo2.txt etc. If path is just "foo.txt," it will return unchanged. If the filepath expands into a directory ignore it.
                        foreach (string expandedFilePath in this.GetResolvedProviderPathFromPSPath(path, out provider)) {
                            if (!Directory.Exists(expandedFilePath)) {
                                filePaths.Add(expandedFilePath);
                            }
                        }
                    }
                    else {
                        // no wildcards, so don't try to expand any * or ? symbols.                    
                        filePaths.Add(this.SessionState.Path.GetUnresolvedProviderPathFromPSPath(path, out provider, out drive));
                    }
                    // ensure that this path (or set of paths after wildcard expansion)
                    // is on the filesystem. A wildcard can never expand to span multiple providers.
					if (provider.ImplementingType != typeof(FileSystemProvider)) {
                        // no, so skip to next path in paths.
                        continue;
                    }
                }

                // At this point, we have a list of paths on the filesystem, process each file. 
                foreach (string filePath in filePaths) {
                    // If the file does not exist display an error and return.
                    if (!File.Exists(filePath)) {
                        FileNotFoundException fileException = new FileNotFoundException("File not found", filePath);
                        ErrorRecord fileNotFoundError = new ErrorRecord(fileException, "FileNotFound", ErrorCategory.ObjectNotFound, filePath);
                        WriteError(fileNotFoundError);
                        return;
                    }

                    // process the file
                    try {
	                    WriteVerbose($"Attempting to extract information from DICOM file: {filePath}...");
	                    var file = DicomFile.Open(filePath);
						
						// save each tag to a hastable, write the hashtable to the pipline
						Hashtable allTags = new Hashtable();
	                    foreach (var tag in file.Dataset)
                    	{
							allTags.Add(tag,file.Dataset.GetValueOrDefault(tag.Tag,0,""));
                    	}
						WriteObject(allTags);
					}
               		catch (Exception e) {
						WriteWarning("Error occurred during DICOM file dump operation. Use -Debug for exception details");
                	    WriteDebug($"Exception: -> {e}");
                	}
				}
			}
		}
	}
}