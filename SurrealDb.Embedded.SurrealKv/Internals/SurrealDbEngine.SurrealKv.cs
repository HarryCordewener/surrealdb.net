﻿using SurrealDb.Embedded.SurrealKv;
using SurrealDb.Net.Internals;

namespace SurrealDb.Embedded.Internals;

internal sealed partial class SurrealDbEmbeddedEngine : ISurrealDbKvEngine
{
    partial void PreConnect()
    {
        // 💡 Create the directory as the Rust bindings do not have the right to do it
        string folderPath = _parameters!.Endpoint!.Replace(
            SurrealDbKvClient.BASE_ENDPOINT,
            string.Empty
        );

        if (!Directory.Exists(folderPath))
        {
            Directory.CreateDirectory(folderPath);
        }
    }
}