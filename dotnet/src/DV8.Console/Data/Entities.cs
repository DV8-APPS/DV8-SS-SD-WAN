using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Net;
using System.Net.NetworkInformation;

namespace DV8.Console.Data
{
    public class Device
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();
        
        [Required]
        public string Tenant { get; set; } = string.Empty;
        
        public string? Kind { get; set; } // router, switch, ap, virtual, gateway
        public string? Vendor { get; set; }
        public string? Model { get; set; }
        public string? SwVersion { get; set; }
        public string? Fingerprint { get; set; }
        
        [Column(TypeName = "TEXT")]
        public string? TrustState { get; set; } // JSON
        
        public string? Site { get; set; }
        public string? Posture { get; set; } // ECC, PQC
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        // Navigation properties
        public virtual ICollection<Interface> Interfaces { get; set; } = new List<Interface>();
        public virtual ICollection<SecretField> SecretFields { get; set; } = new List<SecretField>();
        public virtual ICollection<HoacEvent> HoacEvents { get; set; } = new List<HoacEvent>();
    }

    public class Interface
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();
        
        [Required]
        public Guid DeviceId { get; set; }
        
        public string? Name { get; set; }
        public string? Mac { get; set; } // Physical address
        public string? Ipv4 { get; set; }
        public string? Ipv6 { get; set; }
        public int? SpeedMbps { get; set; }
        public bool? Up { get; set; }
        
        // Navigation properties
        [ForeignKey("DeviceId")]
        public virtual Device Device { get; set; } = null!;
        
        public virtual ICollection<Neighbor> NeighborsA { get; set; } = new List<Neighbor>();
        public virtual ICollection<Neighbor> NeighborsZ { get; set; } = new List<Neighbor>();
    }

    public class Neighbor
    {
        public Guid AIfId { get; set; }
        public Guid ZIfId { get; set; }
        public string Proto { get; set; } = string.Empty; // lldp, cdp, bgp, passive
        public float? Confidence { get; set; }
        
        // Navigation properties
        [ForeignKey("AIfId")]
        public virtual Interface AInterface { get; set; } = null!;
        
        [ForeignKey("ZIfId")]
        public virtual Interface ZInterface { get; set; } = null!;
    }

    public class DiscoveryJob
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();
        
        [Required]
        public string Tenant { get; set; } = string.Empty;
        public bool PassiveOnly { get; set; }
        
        [Column(TypeName = "TEXT")]
        public string? Cidrs { get; set; } // JSON array
        
        public DateTime StartedAt { get; set; } = DateTime.UtcNow;
        public string? ByUser { get; set; }
        public string? DbgSession { get; set; }
        
        // Navigation properties
        public virtual ICollection<DiscoveryResult> Results { get; set; } = new List<DiscoveryResult>();
    }

    public class DiscoveryResult
    {
        public Guid JobId { get; set; }
        public string Fingerprint { get; set; } = string.Empty;
        
        [Column(TypeName = "TEXT")]
        public string? Addrs { get; set; } // JSON array
        
        [Column(TypeName = "TEXT")]
        public string? Signals { get; set; } // JSON array
        
        public float? Confidence { get; set; }
        
        [Column(TypeName = "TEXT")]
        public string? Raw { get; set; } // JSON
        
        // Navigation properties
        [ForeignKey("JobId")]
        public virtual DiscoveryJob Job { get; set; } = null!;
    }

    public class SecretField
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();
        
        public Guid? DeviceId { get; set; }
        public string? FieldName { get; set; }
        public byte[]? Ciphertext { get; set; }
        public byte[]? Iv { get; set; }
        public byte[]? Aad { get; set; }
        public string? LedgerPtr { get; set; }
        
        // Navigation properties
        [ForeignKey("DeviceId")]
        public virtual Device? Device { get; set; }
    }

    public class DbgSession
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();
        
        public string? UserId { get; set; }
        public byte[]? DbgHash { get; set; }
        
        [Column(TypeName = "TEXT")]
        public string? Geo { get; set; } // JSON
        
        public int? RiskFloor { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? ExpiresAt { get; set; }
    }

    public class CcpDecision
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();
        
        public string? Actor { get; set; }
        public string? Action { get; set; }
        
        [Column(TypeName = "TEXT")]
        public string? Context { get; set; } // JSON
        
        public string? Decision { get; set; } // ALLOW, ESCALATE, LOCK, DENY
        public string? Reason { get; set; }
        public string? LedgerHash { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }

    public class HoacEvent
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();
        
        public string? Channel { get; set; }
        public string? Profile { get; set; } // ECC, PQC
        public int? Risk { get; set; }
        public Guid? DeviceId { get; set; }
        public byte[]? Transcript { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        // Navigation properties
        [ForeignKey("DeviceId")]
        public virtual Device? Device { get; set; }
    }
}