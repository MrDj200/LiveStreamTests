namespace CoreWebAPI
{
    public static class FileBuffer
    {
        private const ushort MAX_SIZE = 10;
        private static readonly MemoryStream[] fileBuffer = new MemoryStream[MAX_SIZE];
        private static ulong index;

        public static readonly MemoryStream playlistFile = new();

        static FileBuffer()
        {
            for (uint i = 0; i < MAX_SIZE; i++)
            {
                fileBuffer[i] = new MemoryStream();
            }
        }

        public static MemoryStream GetStream(ulong fileNumber) => fileBuffer[fileNumber % MAX_SIZE];


        /// <summary>
        /// Adds a given stream file to the ring buffer or updates the playlist file
        /// </summary>
        /// <param name="stream">Body of the Request</param>
        /// <param name="fileName">target file name</param>
        /// <returns></returns>
        public static async Task<bool> AddStream(Stream stream, string fileName)
        {
            bool isM3u8 = fileName.EndsWith(".m3u8");
            bool isTs = fileName.EndsWith(".ts");

            if ((!isM3u8 && !isTs) || !stream.CanRead)
            {
                return false;
            }
            if (isM3u8)
            {
                playlistFile.SetLength(0);
                await stream.CopyToAsync(playlistFile);
                return true;
            }
            if (isTs)
            {
                MemoryStream data = fileBuffer[(index++ % MAX_SIZE)];
                data.SetLength(0);
                await stream.CopyToAsync(data);
                return true;
            }

            return false;
        }
    }
}
