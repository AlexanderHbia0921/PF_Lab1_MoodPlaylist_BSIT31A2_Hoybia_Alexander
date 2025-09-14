namespace MoodPlaylistGenerator.Data.Entities
{
    public class PlaylistSong
    {
        public int PlaylistId { get; set; }
        public int SongId { get; set; }
        public int Order { get; set; }

        // Navigation properties
        public Playlist Playlist { get; set; } = null!;
        public Song Song { get; set; } = null!;
    }
}