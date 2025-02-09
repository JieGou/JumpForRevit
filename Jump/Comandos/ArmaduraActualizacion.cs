﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.DB.Structure;
using Autodesk.Revit.ApplicationServices;

namespace Jump
{
    class ArmaduraActualizacion : IUpdater
    {
        // Variable necesarias
        string IdiomaDelPrograma = Tools.ObtenerIdiomaDelPrograma();
        AddInId addinID = null;
        UpdaterId updaterID = null;
        System.Windows.Forms.DataGridView dgvEstiloLinea = new System.Windows.Forms.DataGridView();

        // Contructor de la clase
        public ArmaduraActualizacion(AddInId AddID)
        {
            // Asigna el ID del complemento
            this.addinID = AddID;

            // Crea un nuevo UpdaterId
            this.updaterID = new UpdaterId(addinID, AboutJump.GuidActualizarBarra);
        }

        /// <summary> Modifica la Representación de la armadura cuando existe algún cambio en Revit </summary>
        public void Execute(UpdaterData data)
        {
            // Verifica que esté activo la actualización automatica de barras
            if (Jump.Properties.Settings.Default.ActualizarBarrasAutomaticamente)
            {
                // Documento del proyecto
                Document doc = data.GetDocument();

                // Crea el DataGridView con los diámetros y estilos de líneas
                dgvEstiloLinea = Tools.ObtenerDataGridViewDeDiametrosYEstilos(this.dgvEstiloLinea, doc, this.IdiomaDelPrograma);

                // Obtiene todos los ID de los elementos modificados
                List<ElementId> elementosId = data.GetModifiedElementIds().ToList();

                // Obtiene los elementos modificados
                List<Element> elementos = Tools.ObtenerElementoSegunID(doc, elementosId);

                // Obtiene todas las barras del proyecto
                List<Element> colectorBarras = new FilteredElementCollector(doc).
                                                   OfCategory(BuiltInCategory.OST_Rebar).
                                                   OfClass(typeof(Rebar)).ToList();

                // Obtiene todas las barras modificadas
                List<Element> barrasModificadas = Tools.ObtenerElementosCoincidentesConLista(colectorBarras, elementos);

                List<ArmaduraRepresentacion> listaArmaduraRepresentacion = new List<ArmaduraRepresentacion>();

                // Recorre todas las barras modificadas
                foreach (Rebar barra in barrasModificadas)
                {
                    listaArmaduraRepresentacion.Clear();

                    listaArmaduraRepresentacion = Tools.ObtenerRepresentacionArmaduraDeBarra(barra);

                    Tools.EliminarRepresentacionesEnBarra(barra);

                    // Recorre las Representaciones de armaduras
                    foreach (ArmaduraRepresentacion armadura in listaArmaduraRepresentacion)
                    {
                        try
                        {
                            // Verifica que la barra modificada sea igual al del despiece
                            if (armadura.Barra.Id == barra.Id)
                            {
                                // Verifica que las líneas no sean nulas
                                if (armadura.CurvasDeArmadura != null && armadura.TextosDeLongitudesParciales != null)
                                {
                                    // Elimina el despiece
                                    armadura.Eliminar();

                                    // Dibuja las líneas de la nueva geometría y asigna al objeto
                                    armadura.DibujarArmaduraSegunDatagridview(this.dgvEstiloLinea);

                                    // Mueve la Representación de la Armadura
                                    armadura.MoverArmaduraRepresentacion(armadura.Posicion);

                                    Tools.GuardarRepresentacionArmaduraDeBarra(barra, armadura);                                    
                                }
                            }
                        }
                        catch (Exception)
                        {
                            // Elimina las curvas
                            armadura.Eliminar();
                        }
                    }
                }
            }
        }

        /// <summary> Tipo del cambio que realizará el actualizador </summary>
        public ChangePriority GetChangePriority()
        {
            // Tipo de prioridad
            return ChangePriority.Rebar;
        }

        /// <summary> Devuelve el UpdaterId </summary>
        public UpdaterId GetUpdaterId()
        {
            return updaterID;
        }

        /// <summary> Obtiene la información adicional del actualizador </summary>
        public string GetAdditionalInformation()
        {
            // Información del actualizador
            string info = Language.ObtenerTexto(IdiomaDelPrograma, "ActBar1");

            return info;
        }

        /// <summary> Obtiene el nombre del actualizador </summary>
        public string GetUpdaterName()
        {
            // Nombre del actualizador
            string nombre = Language.ObtenerTexto(IdiomaDelPrograma, "ActBar2");

            return nombre;
        }
    }
}
