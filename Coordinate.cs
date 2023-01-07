using Microsoft.EntityFrameworkCore;
using System;

namespace MachineSystem
{
    
    public class CoordinateContext : DbContext
    {
        public string DB_PATH_NAME;
        public CoordinateContext(string dBPath)
        {                    
            DB_PATH_NAME= dBPath;
        }
        protected override void OnConfiguring(DbContextOptionsBuilder options)
        => options.UseSqlite($"Data Source={DB_PATH_NAME}");

        public DbSet<PointCoordinate> PointCoordinates { get; set; }         

    }
    public class PointCoordinate
    {
        public int Id { get; set; }
        public double x { get; set; }
        public double y { get; set; }
        public double z { get; set; }
        public System.DateTime date { get; set; }
        public System.TimeSpan time { get; set; }
    }

}
