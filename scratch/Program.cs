// using System;
// using System.Collections.Generic;
// using System.Linq;

// namespace LinqDemo;

// public sealed class Album
// {
//     public string Title { get; set; } = string.Empty;
// }

// public sealed class Artist
// {
//     public string Name { get; set; } = string.Empty;
//     public int index { get; set; }
//     public List<Album> Albums { get; set; } = [];
// }

// public static class Program
// {
//     public static void Main()
//     {
//         List<Artist> artists = new List<Artist>
//         {
//             new Artist
//             {
//                 Name = "Metallica",
//                 index = 1,
//                 Albums = new List<Album>
//                 {
//                     new Album { Title = "Ride the Lightning" },
//                     new Album { Title = "Master of Puppets" }
//                 }
//             },
//             new Artist
//             {
//                 Name = "Megadeth",
//                 index = 2,
//                 Albums = new List<Album>
//                 {
//                     new Album { Title = "Rust in Peace" },
//                     new Album { Title = "Countdown to Extinction" }
//                 }
//             }
//         };

//         // 1. Using Select: Projects each Artist's collection of Albums as a nested collection
//         IEnumerable<int> query1 = artists.Select(artist => artist.index);

//         Console.WriteLine("--- Select Output Type: IEnumerable<List<Album>> ---");
//         foreach (int artistName in query1)
//         {
//             Console.WriteLine($"Artist name: {artistName}");
//         }

//         Console.WriteLine();

//         // 2. Using SelectMany: Flattens all Album collections into a single sequence
//         IEnumerable<Album> query2 = artists.SelectMany(artist => artist.Albums);

//         Console.WriteLine("--- SelectMany Output Type: IEnumerable<string> ---");
//         foreach (int artistName in query2)
//         {
//             Console.WriteLine($"- {artistName}");
//         }
//     }
// }
