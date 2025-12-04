import axios from 'axios';

const apiUrl = process.env.REACT_APP_API_URL;

console.log("Using API URL:", apiUrl);

export default {
  getTasks: async () => {
    try {
      const result = await axios.get(`${apiUrl}/items`);
      return result.data;
    } catch (error) {
      console.error("❌ Error fetching tasks:", error);
      return null;
    }
  },

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
