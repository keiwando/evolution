using System;
using Keiwando.NativeFileSO;

public class CustomEvolutionFileType {

	public static readonly SupportedFileType evol = new SupportedFileType {
		Name = "Evolution Save File",
		Extension = "evol",
		Owner = true,
		AppleUTI = "com.keiwando.Evolution.evol",
		AppleConformsToUTI = "public.plain-text",
		MimeType = "application/octet-stream"
	};

	public static readonly SupportedFileType creat = new SupportedFileType {
		Name = "Evolution Creature Save File",
		Extension = "creat",
		Owner = true,
		AppleUTI = "com.keiwando.Evolution.creat",
		AppleConformsToUTI = "public.plain-text",
		MimeType = "application/octet-stream"
	};
}
