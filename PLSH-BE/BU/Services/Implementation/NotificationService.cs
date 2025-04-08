using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using BU.Hubs;
using BU.Models.DTO.Notification;
using BU.Services.Interface;
using Data.DatabaseContext;
using Microsoft.AspNetCore.SignalR;
using Model.Entity.Notification;

namespace BU.Services.Implementation;
public class NotificationService(AppDbContext context, IMapper mapper, IHubContext<ReviewHub> hubContext)
    : INotificationService
{
    public async Task SendNotificationToUserAsync(int userId, NotificationDto notificationDto)
    {
        var notification = new Notification
        {
            Title = notificationDto.Title,
            Content = notificationDto.Content,
            Date = DateTime.UtcNow,
            Reference = notificationDto.Reference ?? "Unknown",
            ReferenceId = notificationDto.ReferenceId ?? 0,
            AccountId = userId,
            IsRead = false
        };

        context.Notifications.Add(notification);
        await context.SaveChangesAsync();

        var resultDto = mapper.Map<NotificationDto>(notification, opt =>
        {
            opt.Items["ReferenceData"] = notificationDto.ReferenceData;
        });

        await hubContext.Clients.User(userId.ToString())
                         .SendAsync("ReceiveNotification", resultDto);
    }

    public async Task SendNotificationToUsersAsync(IEnumerable<int> userIds, NotificationDto notificationDto)
    {
        var now = DateTime.UtcNow;
        var notifications = userIds.Select(userId => new Notification
        {
            Title = notificationDto.Title,
            Content = notificationDto.Content,
            Date = now,
            Reference = notificationDto.Reference ?? "Unknown",
            ReferenceId = notificationDto.ReferenceId ?? 0,
            AccountId = userId,
            IsRead = false
        }).ToList();

        context.Notifications.AddRange(notifications);
        await context.SaveChangesAsync();

        var dtos = notifications.Select(n => mapper.Map<NotificationDto>(n, opt =>
        {
            opt.Items["ReferenceData"] = notificationDto.ReferenceData;
        })).ToList();

        foreach (var dto in dtos)
        {
            await hubContext.Clients.User(dto.AccountId.ToString())
                             .SendAsync("ReceiveNotification", dto);
        }
    }

    public async Task SendNotificationToGroupAsync(string groupName, NotificationDto notificationDto)
    {
        await hubContext.Clients.Group(groupName)
                                 .SendAsync("ReceiveNotification", notificationDto);
    }
}
