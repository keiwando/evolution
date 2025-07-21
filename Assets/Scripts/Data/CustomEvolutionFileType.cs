using System;
using Keiwando.NFSO;

public class CustomEvolutionFileType {

	// TODO: Change the base UTIs here if necessary

	public static readonly SupportedFileType evol = new SupportedFileType {
		Name = "Evolution Save File",
		Extension = "evol",
		Owner = true,
		AppleUTI = "com.keiwando.Evolution.evol",
		AppleConformsToUTI = "public.data",
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

	public static readonly SupportedFileType evolutiongallery = new SupportedFileType {
		Name = "Evolution Gallery Recording",
		Extension = "evolutiongallery",
		Owner = true,
		AppleUTI = "com.keiwando.Evolution.evolutiongallery",
		AppleConformsToUTI = "public.data",
		MimeType = "application/octet-stream"
	};
}
