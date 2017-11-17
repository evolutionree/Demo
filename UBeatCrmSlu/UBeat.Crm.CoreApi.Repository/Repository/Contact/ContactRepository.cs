using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Extensions.Configuration;
using UBeat.Crm.CoreApi.DomainModel.Contact;
using UBeat.Crm.CoreApi.IRepository;

namespace UBeat.Crm.CoreApi.Repository.Repository.Contact
{
    public class ContactRepository:IContactRepository
    {
        public ContactRepository(IConfigurationRoot config)
        {
            
        }
    }
}
