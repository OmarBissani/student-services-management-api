﻿using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Student_Services_Management_App_API.Models;

public class Reservation
{
    [JsonPropertyName("PK_Reservation")]
    [Key]
    public int PK_Reservation { get; set; }

    public int FK_Reservations_TimeSlots { get; set; }
    public int FK_Reservations_Students { get; set; }
}