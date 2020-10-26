using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

public class UiCalendarWindow : EditorWindow, IHasCustomMenu
{
    private static DateTime selectedDate;

    // Add menu named "My Window" to the Window menu
    [MenuItem("Window/Meaf75/UIElements Calendar")]
    static void Init(){
        // Get existing open window or if none, make a new one:
        var window = (UiCalendarWindow) GetWindow(typeof(UiCalendarWindow));
        window.titleContent = new GUIContent("UICalendarWindow");

        window.Show();
    }

    // This interface implementation is automatically called by Unity.
    void IHasCustomMenu.AddItemsToMenu(GenericMenu menu){
        GUIContent content = new GUIContent("Repaint");
        menu.AddItem(content, false, RepaintWindow);
    }

    private void RepaintWindow(){
        Debug.Log("Repainteando");
        VisualElement root = rootVisualElement;
        root.Clear();
        DrawWindow();
    }

    private void OnEnable(){
        selectedDate = DateTime.Now;
        DrawWindow();
    }

    private void DrawWindow(){

        VisualElement root = rootVisualElement;

        var timeRecorderTemplate = Resources.Load<VisualTreeAsset>("CalendarTemplate");
        var timeRecorderTemplateStyle = Resources.Load<StyleSheet>("CalendarTemplateStyle");
        root.styleSheets.Add(timeRecorderTemplateStyle);

        var dayElementTemplate = Resources.Load<VisualTreeAsset>("DayContainerTemplate");
        var dayElementTemplateStyle = Resources.Load<StyleSheet>("DayContainerTemplateStyle");
        root.styleSheets.Add(dayElementTemplateStyle);

        // Add tree to root element
        timeRecorderTemplate.CloneTree(root);

        // Update date label
        var dateLabel = root.Q<Label>("label-date");
        string monthName = $"{selectedDate : MMMM}";
        dateLabel.text = $"{monthName} {selectedDate.Year}";

        // Fix buttons action
        var prevMonthBtn = root.Q<Button>("btn-prev-month");
        var nextMonthBtn = root.Q<Button>("btn-next-month");

        prevMonthBtn.clicked += () => ChangeMonthOffset(-1);
        nextMonthBtn.clicked += () => ChangeMonthOffset(1);

        // Generate days
        var daysContainers = new VisualElement[7];

        // Get reference to all containers before generate elements
        for(int i = 0; i < 7; i++){
            var dayElement = root.Q<VisualElement>("day-"+i);
            daysContainers[i] = dayElement;
        }

        int daysGenerated = 0;

        #region Step 1: Fill previous month
        var firstDay = new DateTime(selectedDate.Year,selectedDate.Month,1);

        if(firstDay.DayOfWeek != DayOfWeek.Monday){
            // Get previous month
            var previousMonthDate = selectedDate.AddMonths(-1);
            previousMonthDate = new DateTime(previousMonthDate.Year,previousMonthDate.Month,1);
            int end = DateTime.DaysInMonth(previousMonthDate.Year, previousMonthDate.Month);
            int reduceDays = firstDay.DayOfWeek == DayOfWeek.Sunday ? 6 : ((int) firstDay.DayOfWeek - 1) % 7;
            int start = end - reduceDays;

            // Generate Elements
            FillDateToDate(daysContainers,previousMonthDate,start,end,ref daysGenerated,true);
        }
        #endregion

        #region Step 2: Fill selected month
            int daysInSelectedMonth = DateTime.DaysInMonth(selectedDate.Year, selectedDate.Month);

            FillDateToDate(daysContainers,selectedDate,0,daysInSelectedMonth,ref daysGenerated);
        #endregion

        #region Step 3: Fill remaining month days
            var selectedMonthFinalDate = new DateTime(selectedDate.Year, selectedDate.Month, daysInSelectedMonth);

            if(selectedMonthFinalDate.DayOfWeek != DayOfWeek.Sunday){
                // In this calendar the final day is "Sunday"
                // so if selected month ends in "Sunday" there is no need to fill remaining days

                var nextMonth = selectedDate.AddMonths(1);
                int remainingDays = 7 - ((int) selectedMonthFinalDate.DayOfWeek);

                FillDateToDate(daysContainers,nextMonth,0,remainingDays, ref daysGenerated,true);
            }
        #endregion
    }

    void FillDateToDate(VisualElement[] daysContainers,DateTime dateSelected, int start, int end, ref int daysGenerated, bool emptyMode = false){

        var dayElementTemplate = Resources.Load<VisualTreeAsset>("DayContainerTemplate");

        // Generate Date elements
        for(int i = start; i < end; i++){
            var date = new DateTime(dateSelected.Year, dateSelected.Month, i + 1);

            var dayContainer = daysContainers[(int) date.DayOfWeek];

            var dayElement = dayElementTemplate.CloneTree();

            var dayLabel = dayElement.Q<Label>("label-day");
            dayLabel.text = emptyMode ? "" : $"{i+1}";

            dayContainer.Add(dayElement);

            daysGenerated++;
        }
    }

    /// <summary> Change month  </summary>
    /// <param name="offset">-1 or 1</param>
    void ChangeMonthOffset(int offset){
        selectedDate = selectedDate.AddMonths(offset);
        RepaintWindow();
    }
}
