﻿using BusinessObject.DTOs.Request;
using BusinessObject.Models;
using Repository.Implements;
using Repository.Interfaces;

namespace IDBMS_API.Services
{
    public class InteriorItemColorService
    {
        private readonly IInteriorItemColorRepository _repository;
        public InteriorItemColorService(IInteriorItemColorRepository repository)
        {
            _repository = repository;
        }
        public IEnumerable<InteriorItemColor> GetAll()
        {
            return _repository.GetAll();
        }
        public InteriorItemColor? GetById(int id)
        {
            return _repository.GetById(id);
        }
        public InteriorItemColor? CreateInteriorItemColor(InteriorItemColorRequest request)
        {
            var iic = new InteriorItemColor
            {
                Name = request.Name,
                Type = request.Type,
                PrimaryColor = request.PrimaryColor,
                SecondaryColor = request.SecondaryColor,
            };
            var iicCreated = _repository.Save(iic);
            return iicCreated;
        }
        public void UpdateInteriorItemColor(int id, InteriorItemColorRequest request)
        {
            var iic = _repository.GetById(id) ?? throw new Exception("This object is not existed!");
            iic.Name = request.Name;
            iic.Type = request.Type;
            iic.PrimaryColor = request.PrimaryColor;
            iic.SecondaryColor = request.SecondaryColor;

           _repository.Update(iic);
        }
        public void DeleteInteriorItemColor(int id)
        {
            var iic = _repository.GetById(id) ?? throw new Exception("This object is not existed!");
            _repository.DeleteById(id);
        }
    }
}
