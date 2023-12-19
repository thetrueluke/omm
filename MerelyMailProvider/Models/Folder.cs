using System;
using System.Collections.Generic;

namespace MerelyMailProvider.Models;

public partial class Folder
{
    public int Id { get; set; }

    public int MailboxId { get; set; }

    public string Name { get; set; } = null!;
}
