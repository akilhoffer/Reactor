param($installPath, $toolsPath, $package, $project) 

Import-Module(join-path $toolsPath "Microsoft.Activities.NuGet.dll")

Remove-ToolboxTab  -Category "Microsoft.Activities"	
Remove-ToolboxTab  -Category "Dictionary"			
