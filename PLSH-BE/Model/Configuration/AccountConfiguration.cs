using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Model.Entity.User;

namespace Model.Configuration;

public class AccountConfiguration : IEntityTypeConfiguration<Account>
{
  public void Configure(EntityTypeBuilder<Account> builder)
  {
    builder.HasIndex(a => a.Email).IsUnique();
    builder.HasIndex(a => a.PhoneNumber).IsUnique();
    builder.HasIndex(a => a.IdentityCardNumber).IsUnique();
  }
}
