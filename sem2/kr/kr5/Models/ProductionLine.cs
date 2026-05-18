namespace kr5.Models;

public class ProductionLine
{
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string Status { get; set; } = "Stopped";

    public float EfficiencyFactor { get; set; } = 1f;

    public bool IsAutomatic { get; set; } = false;

    public int? CurrentWorkOrderId { get; set; }

    public WorkOrder? CurrentWorkOrder { get; set; }

    public List<WorkOrder> WorkOrders { get; set; } = [];
}
