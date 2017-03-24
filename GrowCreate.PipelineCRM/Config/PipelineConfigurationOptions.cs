using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;
using System.Xml;

namespace GrowCreate.PipelineCRM.Config
{
    public class PipelineConfig : ConfigurationSection
    {
        public static PipelineConfig GetConfig()
        {
            return ConfigurationManager.GetSection("PipelineConfig") as PipelineConfig;
        }

        [ConfigurationProperty("appSettings")]
        public PipelineOptions AppSettings 
        { 
            get 
            {
                return (PipelineOptions)this["appSettings"]; 
            } 
            set 
            {
                this["appSettings"] = value; 
            } 
        }

        [ConfigurationProperty("digestBody")]
        public DigestBodyElement DigestBody
        {
            get
            {
                return (DigestBodyElement)this["digestBody"];
            }
            set
            {
                this["digestBody"] = value;
            }
        }

        [ConfigurationProperty("digestRow")]
        public DigestRowElement DigestRow
        {
            get
            {
                return (DigestRowElement)this["digestRow"];
            }
            set
            {
                this["digestRow"] = value;
            }
        }

    }

    public class DigestBodyElement : ConfigurationElement
    {
        public string InnerHtml { get; private set; }
        protected override void DeserializeElement(XmlReader reader, bool serializeCollectionKey)
        {
            InnerHtml = reader.ReadElementContentAsString();
        }
    }

    public class DigestRowElement : ConfigurationElement
    {
        public string InnerHtml { get; private set; }
        protected override void DeserializeElement(XmlReader reader, bool serializeCollectionKey)
        {
            InnerHtml = reader.ReadElementContentAsString();
        }
    }

    public class PipelineOptions : ConfigurationElement
    {
                
        [ConfigurationProperty("createMembers", DefaultValue = true, IsRequired = true)]
        public bool CreateMembers
        {
            get
            {
                return this["createMembers"] is bool && (bool)this["createMembers"];
            }
        }

        [ConfigurationProperty("memberType", IsRequired = true)]
        public string MemberType
        {
            get
            {
                return (string)this["memberType"];
            }
        }

        [ConfigurationProperty("useBoard", DefaultValue = true, IsRequired = true)]
        public bool UseBoard
        {
            get
            {
                return this["useBoard"] is bool && (bool)this["useBoard"];
            }
        }

        [ConfigurationProperty("contactDocTypes", IsRequired = true)]
        public string ContactDocTypes
        {
            get
            {
                return (string)this["contactDocTypes"];
            }
        }

        [ConfigurationProperty("opportunityDocTypes", IsRequired = true)]
        public string OpportunityDocTypes
        {
            get
            {
                return this["opportunityDocTypes"] as string;
            }
        }

        [ConfigurationProperty("organisationDocTypes", IsRequired = true)]
        public string OrganisationDocTypes
        {
            get
            {
                return this["organisationDocTypes"] as string;
            }
        }

        [ConfigurationProperty("segmentDocTypes", IsRequired = true)]
        public string SegmentDocTypes
        {
            get
            {
                return this["segmentDocTypes"] as string;
            }
        }

        [ConfigurationProperty("pageSize", IsRequired = true, DefaultValue = 50)]
        public int PageSize
        {
            get
            {
                return (int)this["pageSize"];
            }
        }

        [ConfigurationProperty("digestTime", IsRequired = true, DefaultValue = 7)]
        public int DigestTime
        {
            get
            {
                return (int)this["digestTime"];
            }
        }

        [ConfigurationProperty("digestSubject", IsRequired = true, DefaultValue = "Pipeline digest")]
        public string DigestSubject
        {
            get
            {
                return (string)this["digestSubject"];
            }
        }

        [ConfigurationProperty("digestSender", IsRequired = true, DefaultValue = "no-reply@website.com")]
        public string DigestSender
        {
            get
            {
                return (string)this["digestSender"];
            }
        }        
    }
}