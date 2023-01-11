using iTextSharp.text;
using iTextSharp.text.pdf;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Windows.Forms;

namespace DivisorPDF
{
    public partial class Form : System.Windows.Forms.Form
    {
        public Form()
        {
            InitializeComponent();

            //Embedded DLL
            AppDomain.CurrentDomain.AssemblyResolve += (sender, args) =>
            {
                string resourceName = new AssemblyName(args.Name).Name + ".dll";
                string resource = Array.Find(this.GetType().Assembly.GetManifestResourceNames(), element => element.EndsWith(resourceName));

                using (var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(resource))
                {
                    Byte[] assemblyData = new Byte[stream.Length];
                    stream.Read(assemblyData, 0, assemblyData.Length);
                    return Assembly.Load(assemblyData);
                }
            };
        }

        private void btnProcessar_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(openFileDialog1.FileName) || !openFileDialog1.FileName.ToUpper().EndsWith(".PDF"))
            {
                MessageBox.Show("Selecione um PDF", "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);                
            }
            else
            {
                //Quantidade Paginas
                PdfReader reader = new PdfReader(openFileDialog1.FileName);
                int qtdPaginas = reader.NumberOfPages;
                reader.Close();
                int tamanhoDivisor = Convert.ToInt32(txtTamanhoDivisor.Text);

                bool validacao = true;

                if (tamanhoDivisor == 0)
                {
                    MessageBox.Show($"Informe a quantidade de página(s) do Divisor", "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    validacao = false;
                }

                if (qtdPaginas == tamanhoDivisor && validacao)
                {
                    MessageBox.Show($"O PDF selecionado contém somente {qtdPaginas} pág.", "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                    validacao = false;
                }

                if (qtdPaginas < tamanhoDivisor && validacao)
                {
                    MessageBox.Show($"O PDF selecionado contem menos pág. do que o divisor", "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                    validacao = false;
                }

                if (validacao)
                {
                    Dictionary<string, string> resultado = DivideArquivo(openFileDialog1.FileName, tamanhoDivisor);
                    MessageBox.Show($"{resultado.Count} Arquivo(s) criado(s) com sucesso! \n\nDiretório: {Path.GetDirectoryName(openFileDialog1.FileName)}", "Sucesso", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }

         
        }

        private void btnSelecionarArquivo_Click(object sender, EventArgs e)
        {
            openFileDialog1.Filter = "Pdf Files|*.pdf";
            openFileDialog1.DefaultExt = ".pdf";
            openFileDialog1.FileName = "";

            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                txtFileSource.Text = openFileDialog1.FileName;
            };

        }
        private Dictionary<String, String> DivideArquivo(string arquivo, int paginaDivisao)
        {
            Dictionary<String, String> arquivoDividido = new Dictionary<String, String>();

            try
            {                

                string arquivoSaida =
                        Path.GetDirectoryName(arquivo)
                        + @"\"
                        + Path.GetFileNameWithoutExtension(arquivo)
                        + "-" + 1
                        + Path.GetExtension(arquivo);
                string[] InFiles = new string[] { arquivo };
                
                Merge(InFiles, arquivoSaida, false);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erro na divisão entre em contato com a TI. Erro: {ex.Message}", "ERRO", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            return arquivoDividido;
        }
        private void txtTamanhoDivisor_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar) 
                )
            {
                e.Handled = true;
            }
        }

        public string Merge(string[] InFiles, string newFile, bool delete)
        {
            var arquivo = "";
            string OutFile = "";

            try
            {
                if (InFiles.Length > 0)
                {
                    int totalFiles = InFiles.GetLength(0);
                    for (int i = 0; i < totalFiles; i++)
                    {
                        arquivo = System.IO.Path.GetFileName(InFiles[i]); 
                    }

                    if (!String.IsNullOrEmpty(newFile)) OutFile = newFile;
                    else OutFile = InFiles[0].Substring(0, InFiles[0].Length - 4) + "-Merge.pdf";

                    using (FileStream stream = new FileStream(OutFile, FileMode.Create))
                    {
                        using (Document doc = new Document())
                        {
                            using (PdfCopy pdf = new PdfSmartCopy(doc, stream))
                            {
                                doc.Open();
                                PdfReader reader = null;
                                PdfReader.unethicalreading = true;
                               
                                int total = InFiles.GetLength(0);
                                for (int i = 0; i < total; i++)
                                {
                                    arquivo = System.IO.Path.GetFileName(InFiles[i]);
                                    reader = new PdfReader(InFiles[i]);                               
                                    pdf.AddDocument(reader);
                                    pdf.FreeReader(reader);
                                    reader.Close();
                                    if (delete) File.Delete(InFiles[i]);
                                }
                            }
                        }
                    }
                    if (String.IsNullOrEmpty(newFile))
                    {
                        File.Delete(InFiles[0]);
                        File.Move(OutFile, InFiles[0]);
                        OutFile = InFiles[0];
                    }
                }
            }
            catch (Exception e)
            {
                throw new FileLoadException($"Erro ao realizar merge do arquivo [{arquivo}]. Erro: {e.Message}");
            }
            return OutFile;
        }

        private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            var url = "https://github.com/mrsric";
            System.Diagnostics.Process.Start(url);
        }
    }
}
