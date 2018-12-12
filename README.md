# ElasticSearch - Indexing PDF content - Using DotnetCore and NEST (official dotnet client)

This sample project uses NEST (high level elasticsearch dot net core client) and shows how to do the following things

1. Create ingest pipeline for ingesting pdf data
2. Create index using settings and mappings (for pdf files)
2. Read pdf from local drive (c:\pdfs) as base64 data
3. Index pdf into elasticsearch so that you can query on pdf content (using kibana or NEST or whatever you would like)

Note that you need to install  [this official plugin]( https://www.elastic.co/guide/en/elasticsearch/plugins/current/ingest-attachment.html)
plugin on your elasticsearch instance , otherwise this app wont work.

Thanks!
