using System;
using System.Collections.Generic;

namespace MerelyMailProvider.Models;

public partial class Mail
{
    public int Id { get; set; }

    public int MailboxId { get; set; }

    public int FolderId { get; set; }

    public string Subject { get; set; } = null!;

    public string? Body { get; set; }

    public string From { get; set; } = null!;

    public string To { get; set; } = null!;

    public int Size { get; set; }
}
