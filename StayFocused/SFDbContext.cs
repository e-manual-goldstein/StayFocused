using Microsoft.EntityFrameworkCore;
using StayFocused.Activities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StayFocused
{
    public class SFDbContext : DbContext
    {

        public SFDbContext(DbContextOptions<SFDbContext> options) : base(options)
        { 
        }

        public DbSet<ActivityRecord> ActivityRecords { get; set; }
    }
}
