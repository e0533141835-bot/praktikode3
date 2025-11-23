import axios from 'axios';

const apiUrl = "http://localhost:5278";

export default {
  // שליפת כל המשימות
  getTasks: async () => {
    const result = await axios.get(`${apiUrl}/items`);
    return result.data;
  },

  // הוספת משימה
  addTask: async (name) => {
    try {
      const result = await axios.post(`${apiUrl}/items`, {
        name,
        isComplete: false
      });
      return result.data;
    } catch (error) {
      console.error("❌ Error adding task:", error);
      return null;
    }
  },

  // עדכון סטטוס של משימה
  setCompleted: async (id, isComplete, name) => {
    try {
      const result = await axios.put(`${apiUrl}/items/${id}`, {
        name,
        isComplete
      });
      return result.data;
    } catch (error) {
      console.error("❌ Error updating task:", error);
      return null;
    }
  },

  // מחיקת משימה
  deleteTask: async (id) => {
    try {
      const result = await axios.delete(`${apiUrl}/items/${id}`);
      return result.data;
    } catch (error) {
      console.error("❌ Error deleting task:", error);
      return null;
    }
  }
};
