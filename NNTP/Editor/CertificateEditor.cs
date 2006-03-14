using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing.Design;
using System.Windows.Forms.Design;
using System.Security.Cryptography.X509Certificates;

namespace Rsdn.Nntp.Editor
{
  class CertificateEditor : UITypeEditor
  {
    public override UITypeEditorEditStyle GetEditStyle(System.ComponentModel.ITypeDescriptorContext context)
    {
      return UITypeEditorEditStyle.Modal;
    }

    public override object EditValue(System.ComponentModel.ITypeDescriptorContext context, IServiceProvider provider, object value)
    {
      IWindowsFormsEditorService service =
        (IWindowsFormsEditorService)provider.GetService(typeof(IWindowsFormsEditorService));

      if (service == null)
        return null;

      X509Store localStore = new X509Store(StoreLocation.LocalMachine);
      try
      {
        localStore.Open(OpenFlags.ReadOnly);

        // find a certificate that is suited for server authentication
        X509Certificate2Collection appropriateCertificates = localStore.Certificates
          .Find(X509FindType.FindByApplicationPolicy, "1.3.6.1.5.5.7.3.1", false)
          .Find(X509FindType.FindByKeyUsage, X509KeyUsageFlags.DataEncipherment, false);
        X509Certificate2Collection collection =
          X509Certificate2UI.SelectFromCollection(appropriateCertificates,
            "Select certificate",
            "Select certificate which will be used for SSL connection authentification and encryption.",
            X509SelectionFlag.SingleSelection);

        if (collection.Count > 0)
          return collection[0];
        else
          return value;
      }
      finally
      {
        localStore.Close();
      }
      //TypeEditorForm editorForm = new TypeEditorForm(value as Type, typeof(IDataProvider));
      //if (service.ShowDialog(editorForm) == DialogResult.OK)
      //{
      //  return editorForm.SelectedType;
      //}
      //else
      //  return value;
    }

  }
}
