﻿using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace GamesToGoAPI.Models
{
    public partial class User
    {
        public User()
        {
            AnswerReport = new HashSet<AnswerReport>();
            Game = new HashSet<Game>();
            Report = new HashSet<Report>();
        }

        [NotMapped]
        public int RoomID { get; set; }
        public int Id { get; set; }
        public string Username { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public int UsertypeId { get; set; }
        public string Image { get; set; }

        public virtual UserType Usertype { get; set; }
        public virtual ICollection<AnswerReport> AnswerReport { get; set; }
        public virtual ICollection<Game> Game { get; set; }
        public virtual ICollection<Report> Report { get; set; }
    }
}