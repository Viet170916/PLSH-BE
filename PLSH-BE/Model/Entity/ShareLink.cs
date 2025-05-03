using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model.Entity
{
    public class ShareLink
    {
        public int Id { get; set; }

        public int BookId { get; set; } 

        public int AccountId { get; set; }

        public string ShareUrl { get; set; }

        public Platform EnablePlatform { get; set; }

        public DateTime CreatedAt { get; set; }

        public string ShortenUrl { get; set; }
    }
    public enum Platform
    {
        Facebook,
        Instagram,
        Zalo
    }
}
