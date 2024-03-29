﻿using BusinessObject.Enums;
using BusinessObject.Models;
using Microsoft.EntityFrameworkCore;
using Repository.Interfaces;
using System.Linq;
using System.Xml.Linq;

namespace Repository.Implements;

public class UserRepository : IUserRepository
{
    public IEnumerable<User> GetAll()
    {
        try
        {
            using var context = new IdtDbContext();
            return context.Users
                .Include(u => u.Comments.Where(cmt => cmt.IsDeleted ==false))
                .Include(u => u.Transactions.Where(trans => trans.IsDeleted == false))
                .Include(u => u.Participations.Where(p => p.IsDeleted == false))
                .Include(u => u.BookingRequests.Where(br => br.IsDeleted == false))
                .OrderByDescending(u => u.CreatedDate)
                .ToList();
        }
        catch
        {
            throw;
        }
}
    public User? GetById(Guid id)
    {
        try
        {
            using var context = new IdtDbContext();
            return context.Users
                .Include(u => u.Comments.Where(cmt => cmt.IsDeleted == false))
                .Include(u => u.Transactions.Where(trans => trans.IsDeleted == false))
                .Include(u => u.Participations.Where(p => p.IsDeleted == false))
                .Include(u => u.BookingRequests.Where(br => br.IsDeleted == false))
                .FirstOrDefault(u => u.Id == id);
        }
        catch
        {
            throw;
        }
    }

    public User? GetByEmail(string email)
    {
        try
        {
            using var context = new IdtDbContext();
            return context.Users
                .Include(u => u.Comments.Where(cmt => cmt.IsDeleted == false))
                .Include(u => u.Transactions.Where(trans => trans.IsDeleted == false))
                .Include(u => u.Participations.Where(p => p.IsDeleted == false))
                .Include(u => u.BookingRequests.Where(br => br.IsDeleted == false))
                .FirstOrDefault(d => d.Email.ToLower().Equals(email.ToLower()));
        }
        catch
        {
            throw;
        }
    }
    public void Lock(string email)
    {
        try
        {
            using var context = new IdtDbContext();
            var user = context.Users.FirstOrDefault(d => d.Email == email);
            user.LockedUntil = DateTime.Now.AddMinutes((int)ConstantValues.LockedTime);
            context.Entry(user).State = Microsoft.EntityFrameworkCore.EntityState.Modified;
            context.SaveChanges();
        }
        catch
        {
            throw;
        }
    }

    public User? Save(User user)
    {
        try
        {
            using var context = new IdtDbContext();
            user.Email = user.Email.ToLower();
            var userAdded = context.Users.Add(user);
            context.SaveChanges();
            return userAdded.Entity;
        }
        catch
        {
            throw;
        }
    }

    public void Update(User user)
    {
        try
        {
            using var context = new IdtDbContext();
            context.Entry(user).State = Microsoft.EntityFrameworkCore.EntityState.Modified;
            context.SaveChanges();
        }
        catch
        {
            throw;
        }
    }

    public void DeleteById(Guid userId)
    {
        try
        {
            using var context = new IdtDbContext();
            User user = new() { Id = userId };
            context.Users.Attach(user);
            context.Users.Remove(user);
            context.SaveChanges();
        }
        catch
        {
            throw;
        }
    }
}
