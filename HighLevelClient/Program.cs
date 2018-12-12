using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using Elasticsearch.Net;
using Nest;

namespace HighLevelClient
{
    public class Document
    {
        public int Id { get; set; }
        public string Path { get; set; }
        public string Content { get; set; }
        public Attachment Attachment { get; set; }
    }


    class Program
    {
        private static string IndexName = "documents";
        private static string RootDirectory = "C:\\pdfs";
        private static string PipeLineName = "pdf_ingester";
        public static string NodeUrl = "http://localhost:9200";
        private static void CreateIndex(ElasticClient client)
        {
            try
            {
                Console.Write($"Creating index {IndexName}...");
                client.CreateIndex(IndexName,
                c => c
                    .Settings(s => s
                        .Analysis(a => a
                            .Analyzers(ad => ad
                                .Custom("windows_path_hierarchy_analyzer", ca => ca
                                    .Tokenizer("windows_path_hierarchy_tokenizer")
                                )
                            )
                            .Tokenizers(t => t
                                .PathHierarchy("windows_path_hierarchy_tokenizer", ph => ph
                                    .Delimiter('\\')
                                )
                            )
                        )
                    )
                    .Mappings(m => m
                        .Map<Document>(mp => mp
                            .AutoMap()
                            .AllField(all => all
                                .Enabled(false)
                            )
                            .Properties(ps => ps
                                .Text(s => s
                                    .Name(n => n.Path)
                                    .Analyzer("windows_path_hierarchy_analyzer")
                                )
                                .Object<Attachment>(a => a
                                    .Name(n => n.Attachment)
                                    .AutoMap()
                                )
                            )
                        )
                    ));
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("DONE");
            }
            catch (Exception)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("ERROR");

            }
            finally
            {
                Console.ResetColor();
            }
        }
        private static void IndexPdfContent(string[] filePaths, string inndexName, ElasticClient client)
        {
            int id = 1;
            foreach (var path in filePaths)
            {
                try
                {
                    Console.Write($"Indexing {path}...");
                    var base64File = Convert.ToBase64String(File.ReadAllBytes(path));
                    client.Index(new Document
                    {
                        Id = id++,
                        Path = path,
                        Content = base64File
                    }, i => i.Pipeline(PipeLineName));
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine("DONE");
                }
                catch (Exception)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("ERROR");
                }
                finally
                {
                    Console.ResetColor();
                }
            }

        }
        private static string[] GetPdfFilePaths(string rootDirectory)
        {
            var files = Directory.GetFiles(rootDirectory, "*.pdf");
            Console.WriteLine($"Total {files.Length} files found!");
            return files;
        }
        private static void CreatePipieline(ElasticClient client)
        {
            try
            {
                Console.Write($"Creating pipeline {PipeLineName}...");

                client.PutPipeline(PipeLineName, p => p
                    .Description("PDF ingester pipeline")
                    .Processors(pr => pr
                        .Attachment<Document>(a => a
                            .Field(f => f.Content)
                            .TargetField(f => f.Attachment)
                        )
                        .Remove<Document>(r => r
                            .Field(f => f.Content)
                        )
                    )
                );
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("DONE");
            }
            catch (Exception)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("ERROR");
            }
            finally
            {
                Console.ResetColor();
            }
        }
        private static ElasticClient CreateEsClient()
        {
            ElasticClient client = null;
            try
            {
                Console.Write("Creating elastic client...");
                var node = new Uri(NodeUrl);
                var settings = new ConnectionSettings(node)
                                    .DefaultIndex(IndexName)
                                    .ThrowExceptions();
                client = new ElasticClient(settings);
                Console.ForegroundColor = ConsoleColor.Green;
                client.Ping();
                Console.WriteLine("DONE");

            }
            catch (System.Exception)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("ERROR");

                Console.WriteLine("Quiting the app. Cannot continue without a valid client.");
                System.Environment.Exit(0);

            }
            return client;

        }
        private static void Start(ElasticClient client)
        {
            CreatePipieline(client);
            CreateIndex(client);
            IndexPdfContent(GetPdfFilePaths(RootDirectory), IndexName, client);
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("Done processing everything. Now you can query your PDF content using kibana or NEST.");
            Console.ResetColor();
        }
        static void Main(string[] args)
        {

            ElasticClient client = CreateEsClient();
            try
            {
                Console.Write($"Deleting pipeline {PipeLineName}...");
                client.DeletePipeline(PipeLineName);
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("DONE");
            }
            catch (System.Exception ex)
            {
                Console.WriteLine(ex.Message);
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("ERROR");
            }
            finally
            {
                Console.ResetColor();
            }

            try
            {
                Console.Write($"Deleting index {IndexName}...");
                client.DeleteIndex(IndexName);
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("DONE");
            }
            catch (System.Exception ex)
            {

                Console.WriteLine(ex.Message);
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("ERROR");
            }
            finally
            {
                Console.ResetColor();
            }


            Start(client);
            Console.Write("press a key to end..");
            Console.ReadKey();

        }


    }
}