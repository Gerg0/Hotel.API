using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Hotel.API.Data.Configurations
{
    public class CountryConfiguration : IEntityTypeConfiguration<Country>
    {
        public void Configure(EntityTypeBuilder<Country> builder)
        {
            builder.HasData(
                new Country{
                    Id = 1,
                    Name = "Jamaica",
                    ShortName = "JM"
                },
                new Country{
                    Id = 2,
                    Name = "Bahamas",
                    ShortName = "BS"
                },
                new Country{
                    Id = 3,
                    Name = "Cayman Island",
                    ShortName = "CI"
                }
            );
        }
    }
}