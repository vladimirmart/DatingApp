using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DatingApp.API.DTOs
{
    public class UserForListDto
    {
        public int Id { get; set; }
        public String Username { get; set; }
        public string Gender { get; set; }
        public int Age { get; set; }
        public DateTime Created { get; set; }
        public DateTime LastActive { get; set; }       
        public string City { get; set; }
        public string Country { get; set; }

        public string PhtoUrl { get; set; }
    }
}
