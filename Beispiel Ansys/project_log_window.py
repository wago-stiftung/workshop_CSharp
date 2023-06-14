# -*- coding: utf-8 -*-
import os
import clr

from logger import log
from project_database import get_db_connection
from project_database.project import Project
clr.AddReference("System.Windows.Forms")
clr.AddReference("System.Drawing")
from System.Windows.Forms import Button, Form, Label, TextBox, View, ListView, ListViewItem, ListView, ListViewItem, FormStartPosition, View, SortOrder
from System.Drawing import Point, Color
clr.AddReference("ListViewColumnSorter")
from CustomSorter import ListViewColumnSorter
from project_api import get_metadata_from_project_filename


class SimulationLogWindow(Form):
    """Class to show last changes to current simulation"""
    selectionWindow = None
    def __init__(self, log):
        """creates a Window with len(log) elements in listview"""
        self.selectionWindow = self
        self.Text = "Log " + get_metadata_from_project_filename()[1]
        self.Width = 700
        self.Height = 500
        self.StartPosition = FormStartPosition.CenterScreen

        self.listview = ListView()
        self.listview.Parent = self
        self.listview.Width = 690
        self.listview.Height = 470
        self.listview.Columns.Add("Id", -2)
        self.listview.Columns.Add("Timestamp", -2)
        self.listview.Columns.Add("SYS Folder", -2)
        self.listview.Columns.Add("Computation ID", -2)
        self.listview.Columns.Add("Description", 100)
        self.listview.Columns.Add("Last Change", -2)
        self.listview.Columns.Add("Duration", -2)
        self.listview.Columns.Add("Validated", -2)
        self.listview.Columns.Add("Images", -2)
        self.listview.View = View.Details
        self.listview.FullRowSelect = True
        project = Project.load_from_db(get_db_connection())
        for index, simulation in enumerate(log, 0):
            revIndex = len(log) - index
            if revIndex < 10:
                revIndexStr = "00" + str(revIndex)
            elif revIndex >= 10 and revIndex < 100:
                revIndexStr = "0" + str(revIndex)
            else:
                revIndexStr = str(revIndex) 
            lbi = ListViewItem(revIndexStr)
            lbi.SubItems.Add(str(simulation.timestamp))
            lbi.SubItems.Add(simulation.sys_folder)
            lbi.SubItems.Add(simulation.computation_id)
            lbi.SubItems.Add(simulation.simulation_description)
            lbi.SubItems.Add(simulation.description)
            lbi.SubItems.Add(str(simulation.duration))
            lbi.SubItems.Add(str(simulation.validated))
            if os.path.isdir(os.path.join(project.export_path, simulation.sys_folder)):
                lbi.SubItems.Add("Click to open Explorer")
            else:
                lbi.SubItems.Add("")
            self.listview.Items.Add(lbi)

        self.colSorter = ListViewColumnSorter()
        self.listview.ListViewItemSorter = self.colSorter
        self.colSorter.Order = SortOrder.Ascending
        self.colSorter.SortColumn = 0
        self.listview.Sort()

        self.listview.ColumnClick += self.changeSorting
        self.listview.MouseClick += self.openExplorer

        self.Resize += self.resizeWindow

    def openExplorer(self, sender, event):
        hitTestInfo = self.listview.HitTest(event.X, event.Y)
        index = hitTestInfo.Item.SubItems.IndexOf(hitTestInfo.SubItem)
        #log(index, self.listbox.SelectedItems[0].SubItems[index].Text)
        if self.listview.SelectedItems[0].SubItems[index].Text == "Click to open Explorer":
            log("open explorer")
            project = Project.load_from_db(get_db_connection())
            upath = os.path.join(project.export_path, self.listview.SelectedItems[0].SubItems[2].Text)
            #log(upath)
            if not os.path.isdir(upath):
                upath = project.export_path
            os.startfile(upath)

    def changeSorting(self, sender, event):
        if (event.Column == self.colSorter.SortColumn):
            # Reverse the current sort direction for this column.
            if self.colSorter.Order == SortOrder.Ascending:
                self.colSorter.Order = SortOrder.Descending
            else:
                self.colSorter.Order = SortOrder.Ascending
        else:
            # Set the column number that is to be sorted; default to ascending.
            self.colSorter.SortColumn = event.Column
            self.colSorter.Order = SortOrder.Ascending

        # Perform the sort with these new sort options.
        self.listview.Sort()

    def form_closing(self, sender, event):
        """prevents the window from closing, to safe the data"""
        #event.Cancel=True
        #self.Hide()
        
    @staticmethod
    def displayForm(log):
        """
        creates a Form with descriptionValue and computation_id Value from Database
        """
        SimulationLogWindow.selectionWindow = SimulationLogWindow(log)
        if not SimulationLogWindow.selectionWindow.Visible:
            SimulationLogWindow.selectionWindow.Show()
        else:
            SimulationLogWindow.selectionWindow.BringToFront()

    def resizeWindow(self, sender, event):
        """resizes element in form to fit window"""
        self.listview.Width = sender.Width - 15
        self.listview.Height = sender.Height - 40