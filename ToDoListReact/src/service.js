// import axios from 'axios';

// // const apiUrl = "http://localhost:5278";
// const apiUrl = "https://todolist-server-k66i.onrender.com" ;

// // console.log("API URL:", apiUrl);

// export default {
//   // שליפת כל המשימות
//   getTasks: async () => {
//     const result = await axios.get(`${apiUrl}/items`);
//     return result.data;
//   },

//   // הוספת משימה
//   addTask: async (name) => {
//     try {
//       const result = await axios.post(`${apiUrl}/items`, {
//         name,
//         isComplete: false
//       });
//       return result.data;
//     } catch (error) {
//       console.error("❌ Error adding task:", error);
//       return null;
      
//     }
//   },

//   // עדכון סטטוס של משימה
//   setCompleted: async (id, isComplete, name) => {
//     try {
//       const result = await axios.put(`${apiUrl}/items/${id}`, {
//         name,
//         isComplete
//       });
//       return result.data;
//     } catch (error) {
//       console.error("❌ Error updating task:", error);
//       return null;
//     }
//   },

//   // מחיקת משימה
//   deleteTask: async (id) => {
//     try {
//       const result = await axios.delete(`${apiUrl}/items/${id}`);
//       return result.data;
//     } catch (error) {
//       console.error("❌ Error deleting task:", error);
//       return null;
//     }
//   }
// };


import axios from 'axios';

axios.defaults.baseURL ="https://todolist-server-k66i.onrender.com" ;
axios.defaults.headers.post["Content-Type"] = "application/json";

axios.interceptors.response.use(
  response => response, 
  error => {
    console.error("Axios error:", error); 
    return Promise.reject(error);
  }
);

export const getTasks = async () => {
  const response = await axios.get("/items"); 
  return response.data;
};

export const addTask = async (name) => {
  const response = await axios.post("/items", { name, isComplete: false });
  return response.data;
};

export const setCompleted = async (todo, isComplete) => {
  return await axios.put(`/items/${todo.id}`, {
    id: todo.id,
    name: todo.name,
    isComplete: isComplete
  });
};

export const deleteTask = async (id) => {
  const response = await axios.delete(`/items/${id}`);
  return response.data;
};

export default {
  getTasks,
  addTask,
  setCompleted,
  deleteTask
};
