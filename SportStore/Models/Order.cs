using System;
using System.Collections.Generic;

namespace SportStore.Models;

public partial class Order
{
    public int Id { get; set; }

    public string OrderList { get; set; } = null!;

    public DateOnly OrderDate { get; set; }

    public DateOnly DeliveryDate { get; set; }

    public int PickupPointId { get; set; }

    public string Client { get; set; } = null!;

    public string Code { get; set; } = null!;

    public string Status { get; set; } = null!;

    public virtual ICollection<OrderProduct> OrderProducts { get; set; } = new List<OrderProduct>();
}
